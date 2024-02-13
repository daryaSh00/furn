using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using furn.Models;
using System.Web.Mvc;


namespace furn.Controllers
{
    
    public class HomeController : Controller

        
    {
        furndata context = new furndata();
             
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult shop()
        {
            return View();
        }


        public ActionResult getpro()
        {
            var res = context.tbl_Product.Select(x => new { x.pkID, x.Name, x.Price,x.img}).ToList();

            return Json(res, JsonRequestBehavior.AllowGet);
        }



        public ActionResult check_login(string name, string family, string phone, string psw)
        {
            int state = 0;
            var user = context.tbl_users.Where(x => x.Phone == phone).SingleOrDefault();
            if (user == null)
            {
                tbl_users nu = new tbl_users();
                nu.Name = name;
                nu.Family = family;
                nu.Phone = phone;
                nu.Password = psw;
                context.tbl_users.Add(nu);
                context.SaveChanges();

               var user2 = context.tbl_users.Where(x => x.Phone == phone).Single();

                Response.Cookies["userid"].Value = user2.pkID.ToString();
                Response.Cookies["userid"].Expires = DateTime.Now.AddDays(300);

                state = 1; // user registered
            }
            else
            {
                Response.Cookies["userid"].Value = user.pkID.ToString();
                Response.Cookies["userid"].Expires = DateTime.Now.AddDays(300);
                if (user.Password == psw) { 
                    state = 1;
                }
                else
                {
                    state = 2;
                }


            }

            return Json(state, JsonRequestBehavior.AllowGet);
        }




        public ActionResult getbasket(string basket)

        {
            string[] items = basket.Split(',');

            int[] pid= Array.ConvertAll(items, s => int.Parse(s));
            //int[] items = int.Parse(basket.Split(','));

            var res = context.tbl_Product.Where(x => pid.Contains(x.pkID)).Select(x=> new {x.pkID,x.Name,x.img,x.Price}).ToList();
            return Json(res, JsonRequestBehavior.AllowGet);


        }

        public ActionResult invoice(string basket)
        {


            if (Request.Cookies["userid"] == null) { return Json(false, JsonRequestBehavior.AllowGet); }
            if (Request.Cookies["userid"].ToString() == "") { return Json(false, JsonRequestBehavior.AllowGet); }


            int state = 0;
            int factornumber = context.tbl_invoice.Max(x => x.FactorNumber);
            factornumber++;
            int price = 0;
           
           
                int userid = int.Parse(Request.Cookies["userid"].Value);

                string[] basket2 = basket.Split(',');

                int[] basket3 = Array.ConvertAll(basket2, s => int.Parse(s));

               var check = context.tbl_invoice.Where(x => x.fkUserID == userid && x.status == false).FirstOrDefault();
            if (check != null)
            {
                factornumber = check.FactorNumber;
                goto label;
            }


                List<tbl_invoice> records = new List<tbl_invoice>();
                

                foreach (int item in basket3)
                {
                    tbl_invoice ni = new tbl_invoice();
                    ni.fkUserID = userid;
                    ni.fkPID = item;
                    ni.status = false;
                    ni.transid = "0";
                    ni.tracking_id = "0";
                    ni.FactorNumber = factornumber;
                    records.Add(ni);

                }

                context.tbl_invoice.AddRange(records);
                context.SaveChanges();
            label:
                  price = context.View_invoice.Where(x => x.FactorNumber == factornumber).Sum(x => x.Price);

                state = 1;



            
            

            return Json(new { state = state, fn = factornumber, price = price }, JsonRequestBehavior.AllowGet);





        }


        public ActionResult login()
        {
            return View();
        }
    }
}