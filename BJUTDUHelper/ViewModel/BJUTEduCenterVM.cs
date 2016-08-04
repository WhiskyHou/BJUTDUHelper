using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using Windows.UI.Xaml.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Net.Http;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;
using Windows.UI.Xaml.Media;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using System.Text.RegularExpressions;
using BJUTDUHelper.BJUTDUHelperlException;
using BJUTDUHelper.Model;

namespace BJUTDUHelper.ViewModel
{
    public class BJUTEduCenterVM : ViewModelBase
    {
        //消息标识码
        private readonly string messageToken = "1";
        //保存登录的用户姓名
        public static string Name { get; set; }
        //主页面功能列表
        public List<Model.EduNavigationModel> EduNavigationList { get; set; } = new List<Model.EduNavigationModel>
        {
            new Model.EduNavigationModel { IconUri="ms-appx:///images//timetable.png",Name="课程表", PageType=typeof(View.BJUTEduScheduleView)},
            new Model.EduNavigationModel { IconUri="ms-appx:///images//test.png",Name="考试查询",PageType=typeof(View.BJUTEduExamView)},
            new Model.EduNavigationModel { IconUri="ms-appx:///images//report.png",Name="成绩查询" ,  PageType=typeof(View.BJUTEduGradeView)},
            new Model.EduNavigationModel { IconUri="ms-appx:///images//school.png",Name="教室查询" }
        };
        //保存当前登录账号信息
        public Model.BJUTEduCenterUserinfo BJUTEduCenterUserinfo { get; set; }
        //教务管理系统信息服务类
        public Service.BJUTEduCenterService _coreService;
        //教务系统网络请求服务类，保存相关的cookie等
        public Service.HttpBaseService _httpService = new Service.HttpBaseService(true);
        //验证码相关类
        public ViewModel.CheckCodeVM CheckCodeVM { get; set; }
        //密码框相关类
        public ViewModel.AccountModifyVM AccountModifyVM { get; set; }
        #region 标识是否能链接到教务管理系统网站
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set { Set(ref _isConnected, value); }
        }
        #endregion
        public BJUTEduCenterVM()
        {
            _coreService = new Service.BJUTEduCenterService();
            CheckCodeVM = new CheckCodeVM();
            CheckCodeVM.CheckCodeSaved += Login;
            CheckCodeVM.CheckCodeRefresh += RefreshChckcode;

            AccountModifyVM = new AccountModifyVM();
            AccountModifyVM.Saved += SaveUserinfo;

        }
        public async void Loaded()
        {
            //加载基本账号信息，用户名，密码
            var studentid = Service.FileService.GetStudentID();
            var users = Service.DbService.GetInfoCenterUserinfo<BJUTEduCenterUserinfo>();
            BJUTEduCenterUserinfo = users.Where(m => m.Username== studentid).FirstOrDefault();

            if (BJUTEduCenterUserinfo == null)
            {
                AccountModifyVM.Open = true;
            }

            IsConnected=await _coreService.GetConnectedStatus(_httpService);

            GetEduTaskInfo();
        }
       
        private Model.EduNavigationModel ClickedItem { get; set; }
        public async void ItemClick(object o, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Model.EduNavigationModel;
            ClickedItem = item;//暂存点击数据

            if (item.PageType == null)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("这只是个饼，而且还没画完O(∩_∩)O", messageToken);
                return;
            }

            //初始化页面传递参数
            View.EduCenterViewParam param = new View.EduCenterViewParam();
            param.BJUTEduCenterUserinfo = BJUTEduCenterUserinfo;
            param.HttpService = _httpService;

            if (BJUTEduCenterUserinfo == null)
            {
                AccountModifyVM.Open = true;//提示重新输入账号
                return;
            }
            //课程表页面特殊，可无网络连接查看
            if (ClickedItem != null && ClickedItem.PageType == typeof(View.BJUTEduScheduleView))
            {
                NavigationVM.DetailFrame.Navigate(ClickedItem.PageType, param);
                return;
            }
            if (IsConnected)//
            {
                var re = await _coreService.GetAuthState(_httpService, BJUTEduCenterUserinfo.Username);
                if (re != true)
                {
                    RefreshChckcode();

                    //CheckCodeVM.OpenCheckCodeDlg = true;
                    //CheckCodeVM.CheckCodeSource = await _coreService.GetCheckCode(_httpService);//获取验证码，非等待，继续执行
                    
                }
                else//已经认证，直接打开
                {
                    if (ClickedItem != null && ClickedItem.PageType != null)
                    {
                        NavigationVM.DetailFrame.Navigate(ClickedItem.PageType, param);
                    }
                }
                
            }
        }

        public async void Login()
        {
            try
            {
                if (BJUTEduCenterUserinfo == null)
                {
                    throw new NullRefUserinfoException("用户名密码不能为空");
                }
                Name = await _coreService.LoginEduCenter(_httpService, BJUTEduCenterUserinfo.Username, BJUTEduCenterUserinfo.Password, CheckCodeVM.CheckCode);

                if (ClickedItem != null && ClickedItem.PageType != null)
                {
                    View.EduCenterViewParam param = new View.EduCenterViewParam
                    {
                        BJUTEduCenterUserinfo = BJUTEduCenterUserinfo,
                        HttpService = _httpService
                    };
                    NavigationVM.DetailFrame.Navigate(ClickedItem.PageType, param);
                }

            }
            catch (NullRefUserinfoException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("请输入用户名和密码", messageToken);
                AccountModifyVM.Open = true;//提示重新输入账号
                AccountModifyVM.Saved -= Login;
                AccountModifyVM.Saved += Login;

            }
            catch (HttpRequestException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("网络错误", messageToken);
            }
            catch (InvalidCheckcodeException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("验证码错误", messageToken);

                //验证码串口显示，并刷新
                RefreshChckcode();

            }
            catch (InvalidUserInfoException)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send("用户名或密码错误", messageToken);

                AccountModifyVM.Open = true;//提示重新输入账号
                AccountModifyVM.Saved -= Login;
                AccountModifyVM.Saved -= RefreshChckcode;
                AccountModifyVM.Saved += RefreshChckcode;
            }
            catch (Exception ex)
            {
                GalaSoft.MvvmLight.Messaging.Messenger.Default.Send(ex.Message, messageToken);
            }
        }
        public async void RefreshChckcode()
        {
            CheckCodeVM.OpenCheckCodeDlg = true;
            CheckCodeVM.CheckCodeSource = await _coreService.GetCheckCode(_httpService);//获取验证码，非等待，继续执行
        }

        public void ChangeUser()
        {
            AccountModifyVM.Open = true;
            AccountModifyVM.Saved -= Login;
            AccountModifyVM.Saved -=RefreshChckcode;
            AccountModifyVM.Saved += Login;

        }
       
        public void SaveUserinfo()
        {
            if (BJUTEduCenterUserinfo == null)
            {
                BJUTEduCenterUserinfo = new Model.BJUTEduCenterUserinfo();
            }

            BJUTEduCenterUserinfo.Username =  AccountModifyVM.Username;
            BJUTEduCenterUserinfo.Password = AccountModifyVM.Password;

            Service.DbService.SaveInfoCenterUserinfo(BJUTEduCenterUserinfo);

            GalaSoft.MvvmLight.Messaging.Messenger.Default.Send<string>("保存成功", messageToken);

        }
       
        #region 获取教学教务基础信息
        public static string Year { get; set; }
        public static int Term { get; set; }
        public static int Week { get; set; }
        public async void GetEduTaskInfo()
        {
            try
            {
                var re = await _coreService.GetEduBasicInfo(_httpService);
                Year = re.Item1;
                Term = re.Item2;
                Week = re.Item3;
            }
            catch
            {
            }
        }
        #endregion

    }
}
