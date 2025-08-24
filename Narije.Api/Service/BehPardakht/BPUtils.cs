namespace Narije.Api.Payment.BehPardakht
{
    public static class BPUtils
    {
        public static string ErrorMessage(int Code)
        {
            switch(Code)
            {
                case 0: return " تراکنش با موفقیت انجام شد";
                case 11: return "شماره کارت نامعتبر است";
                case 12: return "موجودی کافي نیست";
                case 13: return "رمز نادرست است";
                case 14: return "تعداد دفعات وارد کردن رمز بیش از حد مجاز است";
                case 15: return "کارت نامعتبر است";
                case 16: return "دفعات برداشت وجه بیش از حد مجاز است";
                case 17: return "کاربر از انجام تراکنش منصرف شده است";
                case 18: return "تاريخ انقضای کارت گذشته است";
                case 19: return "مبلغ برداشت وجه بیش از حد مجاز است";
                case 111: return "صادر کننده کارت نامعتبر است";
                case 112: return "خطای سويیچ صادر کننده کارت";
                case 113: return "پاسخي از صادر کننده کارت دريافت نشد";
                case 114: return "دارنده کارت مجاز به انجام اين تراکنش نیست";
                case 21: return "پذيرنده نامعتبر است";
                case 23: return "خطای امنیتي رخ داده است";
                case 24: return "اطالعات کاربری پذيرنده نامعتبر است";
                case 25: return "مبلغ نامعتبر است";
                case 31: return "پاسخ نامعتبر است";
                case 32: return "فرمت اطالعات وارد شده صحیح نمي باشد";
                case 33: return "حساب نامعتبر است";
                case 34: return "خطای سیستمي";
                case 35: return "تاريخ نامعتبر است";
                case 41: return "شماره درخواست تکراری است";
                case 42: return "تراکنش يافت نشد";
                case 43: return "قبلا درخواست تایید داده شده است";
                case 44: return "درخواست تایدد يافت نشد";
                case 48: return "تراکنش رزرو شده است";
                case 412: return "شناسه قبض نادرست است";
                case 413: return "شناسه پرداخت نادرست است";
                case 414: return "سازمان صادر کننده قبض نامعتبر است";
                case 415: return "زمان جلسه کاری به پايان رسیده است";
                case 416: return "خطا در ثبت اطالعات";
                case 417: return "شناسه پرداخت کننده نامعتبر است";
                case 418: return "اشکال در تعريف اطالعات مشتری";
                case 419: return "تعداد دفعات ورود اطالعات از حد مجاز گذشته است";
                case 421: return "آی پی نامعتبر است";
                case 51: return "تراکنش تکراری است";
                case 54: return "تراکنش مرجع موجود نیست";
                case 55: return "تراکنش نامعتبر است";
                case 61: return "خطا در واريز";
                case 62: return "مسیر بازگشت به سايت در دامنه ثبت شده برای پذيرنده قرار ندارد ";
                case 98: return "سقف استفاده از رمز ايستا به پايان رسیده است.";
                case 995: return "تعلق کارت بانکي به مشتری احراز نشد";
                default:
                    return ("خطای نامشخص : " + Code);
            }

        }
    }
}
