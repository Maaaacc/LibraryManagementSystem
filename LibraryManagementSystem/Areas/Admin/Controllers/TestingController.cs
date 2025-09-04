using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Areas.Admin.Controllers
{
    public class TestingController : Controller
    {
        // GET: TestingController
        public ActionResult Index()
        {
            return View();
        }

        // GET: TestingController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TestingController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: TestingController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TestingController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TestingController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: TestingController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: TestingController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
