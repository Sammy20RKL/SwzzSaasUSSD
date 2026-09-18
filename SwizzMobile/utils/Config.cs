using SwizzMobile.PolytechWebRef;
using SwizzMobile.Properties;
using System.Net;

namespace SwizzMobile.Utils
{
    public class Config
    {
        //public static MobileService service_;
        //public string url, username, password, domain;

        public Config()
        {
            //service_ = new MobileService();
            //url = Settings.Default.mobileService.ToString();
            //username = Settings.Default.username.ToString();
            //password = Settings.Default.password.ToString();
            //domain = Settings.Default.domain.ToString();

            //service_.Url = url;
            //service_.PreAuthenticate = true;
            //service_.Credentials = new NetworkCredential(username, password, domain);
        }
        public static MobileService service
        {
            get
            {
                string url, username, password, domain;
                url = Settings.Default.mobileService.ToString();
                username = Settings.Default.username.ToString();
                password = Settings.Default.password.ToString();
                domain = Settings.Default.domain.ToString();

                MobileService service_ = new MobileService();
                service_.Url = url;
                service_.PreAuthenticate = true;
                service_.Credentials = new NetworkCredential(username, password, domain);

                return service_;
            }

        }
    }
}