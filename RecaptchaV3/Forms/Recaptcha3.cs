using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Forms.Core;
using Umbraco.Forms.Core.Attributes;
using Umbraco.Forms.Core.Data.Storage;
using Umbraco.Forms.Core.Enums;
using Umbraco.Forms.Core.Models;

namespace RecaptchaV3.Forms
{
    public sealed class Recaptcha3 : FieldType
    {
        public Recaptcha3()
        {
            Id = new Guid("08b8057f-06c9-4ca5-8a42-fd1fc2a46eff");
            Name = "Recaptcha3";
            Description = "Adds a hidden Recaptcha v3 form field to your Umbraco form.";
            Icon = "icon-eye";
            DataType = FieldDataType.String;
            SortOrder = 10;
            SupportsRegex = true;
            HideLabel = true;
        }

        [Setting("Use case", PreValues = "homepage,login,social,e-commerce", Description = "How is the form used?", View = "Dropdownlist")]
        public string UseCase { get; set; }

        [Setting("Score Threshold", Description = "Score threshold value, when left empty 0.5 is used", View = "TextField")]
        public string ScoreThreshold { get; set; }

        public override IEnumerable<string> ValidateField(Form form, Field field, IEnumerable<object> postedValues, HttpContextBase context, IFormStorage formStorage)
        {
            var returnStrings = new List<string>();
            var token = HttpContext.Current.Request["g-recaptcha-response"];
            var secret = Configuration.GetSetting("RecaptchaPrivateKey");
            var client = new WebClient();
            var jsonResult = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, token));
            var obj = JsonConvert.DeserializeObject<CaptchaResponse>(jsonResult);

            double scoreThreshold;
            double.TryParse(ScoreThreshold, out scoreThreshold);

            if (scoreThreshold <= 0 || scoreThreshold > 1)
            {
                scoreThreshold = 0.5;
            }

            if (!obj.Success || obj.Score < scoreThreshold)
            {
                returnStrings.Add("Recaptcha Failed. Try again!");
            }

            return returnStrings;
        }
    }

    public class CaptchaResponse
    {
        public bool Success { set; get; }
        public double Score { get; set; }
    }
}