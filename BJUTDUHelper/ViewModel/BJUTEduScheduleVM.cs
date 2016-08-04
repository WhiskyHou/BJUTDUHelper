
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

        public ViewModel.CheckCodeVM CheckCodeVM { get; set; }
        public AccountModifyVM AccountModifyVM { get; set; }
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
            CheckCodeVM = new CheckCodeVM();
            CheckCodeVM.CheckCodeSaved += CheckCodeSaved;
            CheckCodeVM.CheckCodeRefresh += CheckCodeRefresh;

            AccountModifyVM = new AccountModifyVM();
            AccountModifyVM.Saved += SaveUserinfo;
        }
        public async void Loaded(object param)
        {
            if (param != null)
            {
                View.EduCenterViewParam eduCenterViewParam = param as View.EduCenterViewParam;
                BJUTEduCenterUserinfo = eduCenterViewParam.BJUTEduCenterUserinfo;
                _httpService = eduCenterViewParam.HttpService;
                
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
        }
       
        public async void GetCurrentSchedule(string name,string username)
        {
            string html = string.Empty;
            try
            {
                html=await _coreService.GetCurrentSchedule(_httpService, name, username);   
            }
            catch (HttpRequestException ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("获取数据失败", messageToken);
            }
            catch
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("遇到意外错误", messageToken);
                return;
            }
            try
            {
                var list = Model.ScheduleModel.GetSchedule(html);//获取课表
                Schedule = new Model.ScheduleModel { ScheduleItemList = list, CurrentWeek = ViewModel.BJUTEduCenterVM.Week, };

                Schedule.GetAllWeek();//获取最大周数
                Schedule.SelectedWeek = ViewModel.BJUTEduCenterVM.Week;

                SaveSchedule();
            }
            catch
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("解析数据失败", messageToken);
            }
        }

        

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

        public async void Refresh()
        {
            try
            {
                var re = await _coreService.GetAuthState(_httpService, BJUTEduCenterUserinfo.Username);
                if (re == true)
                {
                    GetCurrentSchedule(Name, BJUTEduCenterUserinfo.Username);
                }
                else
                {
                    CheckCodeRefresh();
                }
            }
            catch(HttpRequestException requestException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("网络错误", messageToken);
            }
            catch (Exception ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(ex.Message, messageToken);
            }
        }

        //保存验证码后登录并导航
        public async void CheckCodeSaved()
        {
            try
            {
                if (BJUTEduCenterUserinfo == null)
                {
                    throw new NullRefUserinfoException("请输入用户名和密码");
                }
                var name = await _coreService.LoginEduCenter(_httpService, BJUTEduCenterUserinfo.Username, BJUTEduCenterUserinfo.Password, CheckCodeVM.CheckCode);

                Name = name;
                GetCurrentSchedule(name, BJUTEduCenterUserinfo.Username);

            }
            catch(NullRefUserinfoException  )
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("请输入用户名和密码", messageToken);
                AccountModifyVM.Open = true;
                AccountModifyVM.Saved += CheckCodeRefresh;
            }
            catch (HttpRequestException requestException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("网络错误", messageToken);
            }
            catch (InvalidCheckcodeException checkcodeExcepiton)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("验证码错误", messageToken);

                CheckCodeRefresh();
            }
            catch (InvalidUserInfoException userInfoException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("用户名或密码错误", messageToken);

                AccountModifyVM.Open = true;
                AccountModifyVM.Saved += CheckCodeRefresh;

            }
            catch (Exception ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(ex.Message, messageToken);
            }
        }
        //刷新验证码的逻辑
        public async void CheckCodeRefresh()
        {
            CheckCodeVM.OpenCheckCodeDlg = true;
            var source = await _coreService.GetCheckCode(_httpService);
            CheckCodeVM.CheckCodeSource = source;
        }

       

       
        //保存用户名密码
        public async void SaveUserinfo()
        {
            if(BJUTEduCenterUserinfo==null)
                BJUTEduCenterUserinfo = new Model.BJUTEduCenterUserinfo();
            BJUTEduCenterUserinfo.Username = AccountModifyVM.Username;
            BJUTEduCenterUserinfo.Password =  AccountModifyVM.Password;

            Service.DbService.SaveInfoCenterUserinfo(BJUTEduCenterUserinfo);
            
            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>("保存成功", messageToken);
            
        }
    }
}