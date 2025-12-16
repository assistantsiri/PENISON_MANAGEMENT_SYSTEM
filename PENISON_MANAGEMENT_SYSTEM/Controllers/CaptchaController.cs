using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace PENISON_MANAGEMENT_SYSTEM.Controllers
{
    public class CaptchaController : Controller
    {
        private const string CaptchaChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int CaptchaLength = 6;

        [HttpGet]
        public ActionResult GetCaptcha()
        {
            // ✅ Prevent caching in Browser + IIS
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));

            // ✅ Generate new captcha
            string captchaCode = GenerateCaptchaCode();
            Bitmap captchaImage = GenerateCaptchaImage(captchaCode);

            // ✅ Store in session (this is the value used for validation)
            Session["CaptchaCode"] = captchaCode;

            // ✅ Convert bitmap to PNG array
            using (MemoryStream ms = new MemoryStream())
            {
                captchaImage.Save(ms, ImageFormat.Png);
                return File(ms.ToArray(), "image/png");
            }
        }

        private string GenerateCaptchaCode()
        {
            StringBuilder code = new StringBuilder();
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[CaptchaLength];
                rng.GetBytes(data);

                for (int i = 0; i < CaptchaLength; i++)
                {
                    char randomChar = CaptchaChars[data[i] % CaptchaChars.Length];
                    code.Append(randomChar);
                }
            }
            return code.ToString();
        }

        private Bitmap GenerateCaptchaImage(string captchaCode)
        {
            Bitmap bitmap = new Bitmap(200, 60);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Font font = new Font("Arial", 24, FontStyle.Bold))
            using (Brush brush = new SolidBrush(Color.Black))
            {
                graphics.Clear(Color.White);
                graphics.DrawString(captchaCode, font, brush, 10, 10);
            }
            return bitmap;
        }
    }
}
