using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            Id = new Guid("c505f0f1-bf81-46ff-869f-37a8881f7ca4");
            Name = "Recaptcha3";
            Description = "Adds a hidden Recaptcha v3 form field to your Umbraco form.";
            Icon = "icon-eye";
            DataType = FieldDataType.String;
            SortOrder = 10;
            HideLabel = true;
        }

        [Setting("Use case", PreValues = "homepage,login,social,e-commerce", Description = "How is the form used?", View = "Dropdownlist")]
        public string UseCase { get; set; }

        [Setting("Score Threshold", Description = "Score threshold value, when left empty 0.5 is used", View = "TextField")]
        public string ScoreThreshold { get; set; }

        public override IEnumerable<object> ProcessSubmittedValue(
            Field field,
            IEnumerable<object> postedValues,
            HttpContextBase context)
        {
            if (postedValues.Any())
            {
                var token = postedValues.First().ToString();
                var secret = Configuration.GetSetting("RecaptchaPrivateKey");
                CaptchaResponse response;
                using (var client = new WebClient())
                {
                    var jsonResult = client.DownloadString(string.Format(
                        "https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secret, token));
                    response = JsonConvert.DeserializeObject<CaptchaResponse>(jsonResult);
                }

                if (response.Success)
                {
                    return new List<object> { response.Score.ToString(CultureInfo.InvariantCulture) };
                }

                var errorCode = response.ErrorCodes.FirstOrDefault();

                return new List<object> { errorCode ?? "failed" };

            }

            return base.ProcessSubmittedValue(field, postedValues, context);
        }

        public override IEnumerable<string> ValidateField(Form form, Field field, IEnumerable<object> postedValues, HttpContextBase context, IFormStorage formStorage)
        {
            var fieldValueObject = field.Values.FirstOrDefault();

            if (fieldValueObject == null) return new List<string> { "Recaptcha token not set." };

            var fieldValue = fieldValueObject.ToString();

            if (fieldValue == "failed") return new List<string> { "Recaptcha failed." };

            if (!double.TryParse(fieldValue, out var score))
            {
                return new List<string> { $"Recaptcha Failed. (error: {fieldValue})" };
            }
            double.TryParse(ScoreThreshold, out var scoreThreshold);

            if (scoreThreshold <= 0 || scoreThreshold > 1)
            {
                scoreThreshold = 0.5;
            }
            if (score < scoreThreshold) return new List<string> { "Recaptcha score too low." };

            return new List<string>();
        }
    }

    public class CaptchaResponse
    {
        public bool Success { set; get; }
        public double Score { get; set; }

        [JsonProperty("error-codes")]
        public IEnumerable<string> ErrorCodes { get; set; }
    }
}