
using BJUTDUHelper.BJUTDUHelperlException;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BJUTDUHelper.ViewModel
{
    public class BJUTEduScheduleVM:ViewModelBase
    {
        private readonly string messageToken = "1";
        private Service.HttpBaseService _httpService;
        private Service.BJUTEduCenterService _coreService;
        private Model.BJUTEduCenterUserinfo BJUTEduCenterUserinfo;
        private Model.ScheduleModel _schedule;
        public Model.ScheduleModel Schedule
        {
            get { return _schedule; }
            set { Set(ref _schedule, value); }
        }

        public  string Name { get; set; }

        private string _yaer;
        private int _term;
        private int _week;
        
        public string Year
        {
            get { return _yaer; }
            set { Set(ref _yaer, value); }
        }
        public  int Term
        {
            get { return _term; }
            set { Set(ref _term, value); }
        }
        public  int Week
        {
            get { return _week; }
            set { Set(ref _week, value); }
        }

        public BJUTEduScheduleVM()
        {
            
            _coreService = new Service.BJUTEduCenterService();
        }
        public async void Loaded(object param)
        {
            if (param != null)
            {
                _httpService = param as Service.HttpBaseService;
            }

            Year = ViewModel.BJUTEduCenterVM.Year;
            Term = ViewModel.BJUTEduCenterVM.Term;
            Week = ViewModel.BJUTEduCenterVM.Week;

            var scedule= await LoadSchedule();
            if (scedule != null)
            {
                if (Schedule == null)
                {
                    Schedule = new Model.ScheduleModel();
                }
               
                Schedule.ScheduleItemList = scedule.ScheduleItemList;
                Schedule.Weeks = scedule.Weeks;
                Schedule.AllWeek = scedule.AllWeek;

                if (ViewModel.BJUTEduCenterVM.Week != 0)
                {
                    Schedule.CurrentWeek = ViewModel.BJUTEduCenterVM.Week;
                    Schedule.SelectedWeek = ViewModel.BJUTEduCenterVM.Week;
                }
                else
                {
                    Schedule.CurrentWeek = scedule.CurrentWeek;
                    Schedule.SelectedWeek = scedule.SelectedWeek;
                }
                
                
            }
            BJUTEduCenterUserinfo= await Manager.AccountManager.GetAccount<Model.BJUTEduCenterUserinfo>();
        }
       
        public async void GetCurrentSchedule(string name,string username)
        {
            try
            {
                string re;
                re = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/xskbcx.aspx?xh=" + username + "&xm=" + name + "&gnmkdm=N121603", HttpMethod.Get, referUri: "http://gdjwgl.bjut.edu.cn/xs_main.aspx?xh=" + username);
               
                var list= Model.ScheduleModel.GetSchedule(re);//获取课表
                Schedule = new Model.ScheduleModel { ScheduleItemList = list, CurrentWeek = ViewModel.BJUTEduCenterVM.Week ,   };
                
                Schedule.GetAllWeek();//获取最大周数
                Schedule.SelectedWeek = ViewModel.BJUTEduCenterVM.Week;

                SaveSchedule();
            }
            catch (Exception ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("解析Schedule出错", messageToken);
            }
        }

        //public async void GetHistorySchedule(string name, string username,string year,string term)
        //{
        //    try
        //    {
        //        string re;
        //        re = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/xskbcx.aspx?xh=" + username + "&xm=" + name + "&gnmkdm=N121603", HttpMethod.Get, referUri: "http://gdjwgl.bjut.edu.cn/xs_main.aspx?xh=" + username);
        //        //re = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/xs_main.aspx?xh=" + username, HttpMethod.Get);


        //        string __VIEWSTATEString;
        //        __VIEWSTATEString = Service.BJUTEduCenterService.GetViewstate(re);

        //        IDictionary<string, string> parameters = new Dictionary<string, string>();
        //        parameters.Add("__EVENTTARGET", "xqd");
        //        parameters.Add("__EVENTARGUMENT", "");
        //        parameters.Add("__VIEWSTATE", __VIEWSTATEString);
        //        parameters.Add("xnd", year);
        //        parameters.Add("xqd", term);

        //        re = await _httpService.SendRequst("http://gdjwgl.bjut.edu.cn/xskbcx.aspx?xh=" + username + "&xm=" + name + "&gnmkdm=N121603", HttpMethod.Post, parameters);

        //        if (Schedule == null)
        //        {
        //            Schedule = new Model.ScheduleModel();
        //        }
        //        Schedule.GetSchedule(re);
        //    }
        //    catch (Exception ex)
        //    {
        //        GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("解析Schedule出错", messageToken);
        //    }
        //}

        #region 本地管理逻辑
        public async void SaveSchedule()
        {
            DAL.LocalSetting _localSetting = new DAL.LocalSetting();
            await _localSetting.SetLocalInfo<Model.ScheduleModel>(typeof(Model.ScheduleModel).Name, Schedule);
        }

        private object lockObject = new object();
        public async Task<Model.ScheduleModel> LoadSchedule()
        {
            DAL.LocalSetting _localSetting = new DAL.LocalSetting();
            var scheduleModel = await _localSetting.GetLocalInfo<Model.ScheduleModel>(typeof(Model.ScheduleModel).Name);
            return scheduleModel;
        }
        #endregion


        #region 验证码框
        private string _checkCode;
        public string CheckCode
        {
            get { return _checkCode; }
            set { Set(ref _checkCode, value); }
        }
        private ImageSource _checkCodeSource;
        public ImageSource CheckCodeSource
        {
            get { return _checkCodeSource; }
            set { Set(ref _checkCodeSource, value); }
        }
        private bool _openCheckCodeDlg;
        public bool OpenCheckCodeDlg
        {
            get { return _openCheckCodeDlg; }
            set { Set(ref _openCheckCodeDlg, value); }
        }

        public async void Refresh()
        {
            var re = await GetAuthState();
            if (re == true)
            {
                GetCurrentSchedule(Name, BJUTEduCenterUserinfo.Username);
            }
            else
            {
                var source = await _coreService.GetCheckCode(_httpService);
                CheckCodeSource = source;
                OpenCheckCodeDlg = true;
            }
        }

        /// <summary>
        /// 保存验证码后登录并导航
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public async void CheckCodeSaved(object o, EventArgs e)
        {
            try
            {
                if (BJUTEduCenterUserinfo == null)
                {
                    throw new NullRefUserinfoException("请输入用户名和密码");
                }
                var name = await _coreService.LoginEduCenter(_httpService, BJUTEduCenterUserinfo.Username, BJUTEduCenterUserinfo.Password, CheckCode);

                Name = name;
                GetCurrentSchedule(name, BJUTEduCenterUserinfo.Username);

            }
            catch(NullRefUserinfoException  )
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("请输入用户名和密码", messageToken);
                Saved = new Action<object>(SaveUserinfo);
                Saved += (arg) => { CheckCodeSaved(null, null); };
            }
            catch (HttpRequestException requestException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("网络错误", messageToken);
            }
            catch (InvalidCheckcodeException checkcodeExcepiton)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("验证码错误", messageToken);

                OpenCheckCodeDlg = true;
                CheckCodeSource = await _coreService.GetCheckCode(_httpService);//获取验证码，非等待，继续执行

            }
            catch (InvalidUserInfoException userInfoException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("用户名或密码错误", messageToken);

                Open = true;
                Saved = new Action<object>(SaveUserinfo);
                Saved += (arg) => { OpenCheckCodeDlg = true;CheckCodeRefresh(null, null); };

            }
            catch (Exception ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(ex.Message, messageToken);
            }
        }
        public async void CheckCodeRefresh(object o, EventArgs e)
        {
            var source = await _coreService.GetCheckCode(_httpService);
            CheckCodeSource = source;
        }

        #endregion

        #region 检测是否已经认证过教务系统
        public async Task<bool> GetAuthState()
        {
            if (BJUTEduCenterUserinfo == null)
            {
                return false;
            }
               var re = await _httpService.GetResponseCode(_coreService.checckAuthUri + BJUTEduCenterUserinfo.Username);
            if (re == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }
        #endregion

        #region 用户名密码框逻辑代码
        //控制用户名密码框的状态
        private bool _open;
        public bool Open
        {
            get { return _open; }
            set { Set(ref _open, value); }
        }
        private Action<object> _saved;
        public Action<object> Saved
        {
            get { return _saved; }
            set { Set(ref _saved, value); }
        }
        //保存用户名密码
        public async void SaveUserinfo(object o)
        {
            var user = o as Model.UserBase;
            if(BJUTEduCenterUserinfo==null)
                BJUTEduCenterUserinfo = new Model.BJUTEduCenterUserinfo();
            BJUTEduCenterUserinfo.Username = user.Username;
            BJUTEduCenterUserinfo.Password = user.Password;

            try
            {
                Manager.AccountManager.SetAccount(BJUTEduCenterUserinfo);
            }
            catch (NullReferenceException nullRef)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>(nullRef.Message, messageToken);
            }
            catch
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>("保存失败", messageToken);
            }

        }
        #endregion
    }
}
