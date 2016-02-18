﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using helpdesk.Models;
using System.Data;

namespace helpdesk.Controllers
{
    [Authorize]
    public class RequestController : Controller
    {
        
        HelpDeskContext db = new HelpDeskContext();
        //
        // GET: /Request/

        public ActionResult Index()
        {
            User user = db.Users.Where(m => m.Login == HttpContext.User.Identity.Name).FirstOrDefault();
            var requests = db.Requests.Where(r => r.User.Id == user.Id)
                .Include(r => r.Category)
                .Include(r => r.Lifecycle)
                .Include(r => User)
                .OrderByDescending(r => r.Lifecycle.Opened);
            return View(requests.ToList());
        }

        [HttpGet]
        public ActionResult Create()
        {
            User user = db.Users.Where(m=>m.Login == HttpContext.User.Identity.Name).FirstOrDefault();
            if (user != null)
            {
                var cabs = from cab in db.Activs
                           where cab.DepartmentId == user.DepartmentId
                           select cab;
                ViewBag.Cabs = new SelectList(cabs, "Id", "CabNumber");
                ViewBag.Categories = new SelectList(db.Categories, "Id", "Name");
                
                return View();
            }
            return RedirectToAction("LogOff", "Account");
        }

        [HttpPost]
         public ActionResult Create(Request request, HttpPostedFileBase error)
        {
            User user = db.Users.Where(m => m.Login == HttpContext.User.Identity.Name).FirstOrDefault();
            if (user == null)
            {
                RedirectToAction("LogOff", "Account");
            }
            if (ModelState.IsValid)
            {
                request.Status = (int)RequestStatus.Open;
                DateTime current = DateTime.Now;

                Lifecycle newlifecycle = new Lifecycle() {Opened = current};
                request.Lifecycle = new Lifecycle();

                db.Lifecycles.Add(newlifecycle) ;
                request.UserId = user.Id;

                if (error!=null)
                {
                    string ext = error.FileName.Substring(error.FileName.LastIndexOf('.'));

                   string path = current.ToString("dd/MM/yyyy H:mm:ss".Replace(":",".").Replace("/", ".") + ext);
                    error.SaveAs(Server.MapPath("~/Files/" + path));
                   request.File = path;
                }
                db.Requests.Add(request);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(request);
        }

        public ActionResult Details(int id)
        {
            Request request = db.Requests.Find(id);
            if (request != null)
            {
                var activ = db.Activs.Where(m => m.Id == request.Activ.Id);
                if (activ.Count()>0)
                {
                    request.Activ = activ.First();
                }

                request.Category = db.Categories.Where(m => m.Id == request.CategoryId).First();
                return PartialView("_Details", request);
            }
            return View("Index");
        }

        public ActionResult Executor(int id)
        {
            User executor = db.Users.Where(m => m.Id == id).First();

            if (executor != null)
            {
                return PartialView("_Executor", executor);
            }
            return View("Index");
        }

        public ActionResult Lifecycle(int id)
        {
            Lifecycle lifecycle = db.Lifecycles.Where(m => m.Id == id).First();

            if (lifecycle != null)
            {
                return PartialView("_Lifecycle", lifecycle);
            }
            return View("Index");
        }

        public ActionResult Delete (int id)
        {
            Request request = db.Requests.Find(id);

            User user = db.Users.Where(m => m.Login == HttpContext.User.Identity.Name).First();
            if (request != null && request.UserId==user.Id)
            {
                Lifecycle lifecycle = db.Lifecycles.Find(request.LifecycleId);
                db.Lifecycles.Remove(lifecycle);
                db.SaveChanges();

            }
            return RedirectToAction("Index");
        }

        public ActionResult Download(int id)
        {
            Request r = db.Requests.Find(id);
            if (r != null)
            {
                string filename = Server.MapPath("~/Files/" + r.File);
                string contentType = "image/jpeg";

                string ext = filename.Substring(filename.LastIndexOf('.'));
                switch (ext)
                {
                    case "txt":
                        contentType = "text/plain";
                        break;
                    case "png":
                        contentType = "image/png";
                        break;
                    case "tiff":
                        contentType = "image/tiff";
                        break;
                }
                return File(filename, contentType, filename);
            }

            return Content("Файл не найден");
        }

        [Authorize(Roles = "Администратор")]
        public ActionResult RequestList()
        {
            var requests = db.Requests.Include(r => r.Category)
                                    .Include(r => r.Lifecycle)
                                    .Include(r => r.User)
                                    .OrderByDescending(r => r.Lifecycle.Opened);

            return View(requests.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Модератор")]
        public ActionResult Distribute()
        {
            var requests = db.Requests.Include(r => r.User)
                                    .Include(r => r.Lifecycle)
                                    .Include(r => r.Executor)
                                    .Where(r => r.ExecutorId == null)
                                    .Where(r => r.Status != (int)RequestStatus.Closed);
            List<User> executors = db.Users.Include(e => e.Role)
                                        .Where(e => e.Role.Name == "Исполнитель").ToList<User>();

            ViewBag.Executors = new SelectList(executors, "Id", "Name");
            return View(requests);
        }

        [HttpPost]
        [Authorize(Roles = "Модератор")]
        public ActionResult Distribute(int? requestId, int? executorId)
        {
            if (requestId == null && executorId == null)
            {
                return RedirectToAction("Distribute");
            }
            Request req = db.Requests.Find(requestId);
            User ex = db.Users.Find(executorId);
            if (req == null && ex == null)
            {
                return RedirectToAction("Distribute");
            }
            req.ExecutorId = executorId;

            req.Status = (int)RequestStatus.Distributed;
            Lifecycle lifecycle = db.Lifecycles.Find(req.LifecycleId);
            lifecycle.Distributed = DateTime.Now;
            db.Entry(lifecycle).State = EntityState.Modified;

            db.Entry(req).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Distribute");
        }

        [HttpGet]
        [Authorize(Roles="Исполнитель")]
        public ActionResult ChangeStatus()
        {
            User user = db.Users.Where(m => m.Login == HttpContext.User.Identity.Name).First();
            if (user!=null)
            {
                var requests = db.Requests.Include(r => r.User)
                    .Include(r => r.Lifecycle)
                    .Include(r => r.Executor)
                    .Where(r => r.ExecutorId == user.Id)
                    .Where(r => r.Status != (int)RequestStatus.Closed);
                return View(requests);
            }
            return RedirectToAction("LogOff", "Account");
        }

        [HttpPost]
        [Authorize(Roles="Исполнитель")]
        public ActionResult ChangeStatus(int requestId, int status)
        {
            User user = db.Users.Where(m => m.Login == HttpContext.User.Identity.Name).First();
            if (user==null)
            {
                return RedirectToAction("LogOff", "Account");
            }
            Request req = db.Requests.Find(requestId);
            if (req!=null)
            {
                req.Status = status;
                Lifecycle lifecycle = db.Lifecycles.Find(req.LifecycleId);
                if (status==(int)RequestStatus.Proccesing)
                {
                    lifecycle.Proccesing = DateTime.Now;
                }
                else if (status == (int)RequestStatus.Checking)
                {
                    lifecycle.Checking = DateTime.Now;
                }
                else if (status == (int)RequestStatus.Closed)
                {
                    lifecycle.Closed = DateTime.Now;
                }

                db.Entry(lifecycle).State = EntityState.Modified;
                db.Entry(req).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ChangeStatus");
        }
    }
}
