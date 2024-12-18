//
//  Unity_iOS_Connect.m
//  BSFTestDemo
//
//  Created by nd on 2022/7/20.
//

#import <Foundation/Foundation.h>


#import <TQOverSeasCommonSdkNDOverseas/TQOverSeasCommonSdk.h>
#import "DemoUtil.h"
#import "RechargeViewController.h"

#pragma mark SDK回调对象

//unity回调，函数指针
typedef void (*MessageResultHandler) (int code, const char *msg);//初始化操作后的回调

@interface OverSeasCallbackHandle : NSObject<TQOverSeasCallbackHandleDelegate,TQOverSeasChannelCallbackHandleDelegate>

@property (nonatomic, assign) MessageResultHandler onInitResultHandler;
@property (nonatomic, assign) MessageResultHandler onLoginResultHandler;
@property (nonatomic, assign) MessageResultHandler onLogoutResultHandler;
@property (nonatomic, assign) MessageResultHandler onRechaegeResultHandler;
@property (nonatomic, assign) MessageResultHandler onExitResultHandler;
@property (nonatomic, assign) MessageResultHandler onExtraDataResultHandler;
@property (nonatomic, assign) MessageResultHandler onBuiltinActionResultHandler;
 
+ (instancetype)callbackHandle;

@end

@implementation OverSeasCallbackHandle

+ (instancetype)callbackHandle {
    static OverSeasCallbackHandle *callbackHandle  = nil;

    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        callbackHandle = [[OverSeasCallbackHandle alloc] init];
    });
    return callbackHandle;
}

- (int)platformId {
    return 1090;;
}

- (NSString *)appId {
    NSString *appid = [[NSUserDefaults standardUserDefaults] objectForKey:@"NDSwitchAppid"];
    if (!appid) {
        appid = @"301001";
    }
    return  appid;
}

- (void)loginCheck:(TQOverSeasLoginResultModel *)result {
    NSString *domain = nil;
    domain = [DemoUtil networkType]==0?@"https://hw-unisdk.99.com":@"http://192.168.182.146:8087";
#ifdef XCJH
    domain = [DemoUtil networkType]==0?@"https://uni-sdk-xcjh.99.com":@"http://192.168.182.146:8087";
#endif
    NSString *urlString = [NSString stringWithFormat:@"%@/LoginCheck.html?PlatformId=%d&AppId=%@&AccountId=%@&SessionId=%@&SessionSecret=%@",domain
                           , [self platformId],[self appId], result.accountId?:@"", result.accountToken?:@"", result.accountTokenSecret?:@""
                           ];
    NSLog(@"---%@", urlString);
    
    urlString = [urlString  stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
    
    NSURL* requestURL = [NSURL URLWithString:urlString];
    
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:requestURL
                                                           cachePolicy:NSURLRequestUseProtocolCachePolicy
                                                       timeoutInterval:8];
    request.HTTPMethod = @"GET";
    NSURLSession *session = [NSURLSession sharedSession];
    NSURLSessionDataTask *task = [session dataTaskWithRequest:request completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
        dispatch_async(dispatch_get_main_queue(), ^{
            if (error) {

                self.onLoginResultHandler(0,[[error localizedDescription] UTF8String]);
                
            }
            else {
                self.onLoginResultHandler(1,[[[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding] UTF8String]);
            }
        });
    }];
    [task resume];
}

#pragma mark delegate

- (void)TQOverSeasHandleInitResult:(TQOverSeasInitResultModel *)result {
    if (result.error) {
        if (self.onInitResultHandler) {
            self.onInitResultHandler(1,[[NSString stringWithFormat:@"初始化失败:%@",result.error] UTF8String]);
        }
    }
    else {
        if (self.onInitResultHandler) {
            self.onInitResultHandler(1,[@"初始化成功" UTF8String]);
        }
        NSInteger index = [DemoUtil networkType];
        NSString *network = @[@"外网",@"内网"][index];
        if (self.onExtraDataResultHandler) {
            self.onExtraDataResultHandler(1,[[NSString stringWithFormat:@"当前网络环境:%@",network] UTF8String]);
        }
    }
}

- (void)TQOverSeasHandleLoginResult:(TQOverSeasLoginResultModel *)result {
    if (result.error) {
        if (result.error.code == -12) {
        }
        else {
        }
    }
    else {
        [self loginCheck:result];

        //进入游戏必须调用
        TQOverSeasRoleInfoModel *roleInfo = [TQOverSeasRoleInfoModel new];
        roleInfo.serverId = @"3113";
        roleInfo.serverName = @"server";
        roleInfo.roleId = @"1";
        roleInfo.roleName = @"role";
        [[TQOverSeasLogicCenter shareLogicCenter] PostRoleInfo:roleInfo event:EventTypeEnterGame];

    }
}

- (void)TQOverSeasHandleLogoutResult:(TQOverSeasLogoutResultModel *)result {
    if (result.error) {
//        [self textViewAddMessage:[NSString stringWithFormat:@"登出失败:%@",result.error]];
    }
    else {
//        [self textViewAddMessage:@"登出成功"];
    }
}

- (void)TQOverSeasHandleRechargeResult:(TQOverSeasRechargeResultModel *)result {
    if (result.error) {
//        [self textViewAddMessage:[NSString stringWithFormat:@"充值失败:%@",result.error]];
    }
    else {
//        [self textViewAddMessage:@"充值成功"];
    }
}

@end

#ifdef __cplusplus
extern "C" {
#endif
/// 加载配置信息
extern void loadSetupInfo() {

    NSInteger network = [DemoUtil networkType];

    // 设置内外网
    [[TQOverSeasLogicCenter shareLogicCenter] setNetworkState:network];

    NSString *appid = @"303001";
    if ([appid isEqualToString:@"303101"]) {//MY
        // Facebook appid  303001/189505031501742 301018/2423669861011925
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"294919971526895" universalLink:nil platformType:UNI_LOGIN_PLATFORM_FACEBOOK];
    }
    else if ([appid isEqualToString:@"303001"]) {//R2
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"189505031501742" universalLink:nil platformType:UNI_LOGIN_PLATFORM_FACEBOOK];
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"7590012" universalLink:nil platformType:UNI_LOGIN_PLATFORM_VK];
    }
    else if ([appid isEqualToString:@"301018"]) {//SLG
        // Facebook appid  303001/189505031501742 301018/2423669861011925
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"189505031501742" universalLink:nil platformType:UNI_LOGIN_PLATFORM_FACEBOOK];
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"1566274253" universalLink:nil platformType:UNI_LOGIN_PLATFORM_LINE];
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"7590012" universalLink:nil platformType:UNI_LOGIN_PLATFORM_VK];
    }
    else if ([appid isEqualToString:@"302008"]) {//COP
        // Facebook appid  303001/189505031501742 301018/2423669861011925
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"2244262309044433" universalLink:nil platformType:UNI_LOGIN_PLATFORM_FACEBOOK];
    }
    else if ([appid isEqualToString:@"302008"]) {//BJST
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"2941462456173679" universalLink:nil platformType:UNI_LOGIN_PLATFORM_FACEBOOK];
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"7892484" universalLink:nil platformType:UNI_LOGIN_PLATFORM_VK];
    }
    else if ([appid isEqualToString:@"303202"]) {//消除计划
        // Facebook appid  303001/189505031501742 301018/2423669861011925
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"194105409349675" universalLink:nil platformType:UNI_LOGIN_PLATFORM_FACEBOOK];
    }

    else {// 301001
        // Wechat appid wxa041e0b14aada4ec
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"wxe0ff53b73797dbbd" universalLink:@"https://slayzp.jmlk.co/1de62739f5556851d347fa98" platformType:UNI_LOGIN_PLATFORM_WECHAT];
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"2423669861011925" universalLink:nil platformType:UNI_LOGIN_PLATFORM_FACEBOOK];
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"1553840658" universalLink:nil platformType:UNI_LOGIN_PLATFORM_LINE];

        // LINE appid  301001/1553840658  301018/1566274253
        //    [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"1566274253" universalLink:nil platformType:UNI_LOGIN_PLATFORM_LINE];
            // instagram appid
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"b5c6a734c1bb48a6bce4b784787c0eca" universalLink:nil platformType:UNI_LOGIN_PLATFORM_INSTAGRAM];
        // VTC appid
        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"53d4242dc8499795412493a7dc34ce90" universalLink:nil platformType:UNI_LOGIN_PLATFORM_VTC];

        NSString *path = [[NSBundle mainBundle] pathForResource:@"bind_gift.png" ofType:nil];
        [[TQOverSeasLogicCenter shareLogicCenter] setGiftImagePath:path];

        [[TQOverSeasLogicCenter shareLogicCenter] setAppid:@"7590012" universalLink:nil platformType:UNI_LOGIN_PLATFORM_VK];
    }
    // 设置关闭按钮状态
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldShowLoginCloseButton:YES];
    // 设置三方平台按钮状态
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldShowThirdLoginButton:YES];
    // 设置游客登录按钮状态
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldShowTouristLoginButton:YES];
    // 设置三方平台按钮状态
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldTouristLoginFirstOpen:[[[NSUserDefaults standardUserDefaults] valueForKey:@"NDShouldTouristLoginFirstOpen"] boolValue]];
    // 设置三方平台按钮状态
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldGuideToBindThirdWhenLogin:YES];
    // 设置三方平台按钮状态
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldGuideToBindThirdWhenRecharge:YES];

    // 设置帐号列表显示状态
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldHiddenAccountList:YES];
    // 设置开启礼品图片按钮
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldShowBindGiftImageView:YES];

    // 设置背景
    [[TQOverSeasLogicCenter shareLogicCenter] setLoginUIBackgroundVisible:[[[NSUserDefaults standardUserDefaults] valueForKey:@"NDShowLoginBackground"] boolValue]];
    // 设置日志
//    [[TQOverSeasLogicCenter shareLogicCenter] setLogType:2 logDir:nil logFileName:[[NSUserDefaults standardUserDefaults] valueForKey:@"NDSetLogName"]];
    // 设置皮肤
    [[TQOverSeasLogicCenter shareLogicCenter] setLoginMainView:[[[NSUserDefaults standardUserDefaults] valueForKey:@"NDSetMainLoginView"] intValue]];
    // 设置是否显示快速注册
    [[TQOverSeasLogicCenter shareLogicCenter] setQuickRegister:YES];
    // 设置显示版本号
    [[TQOverSeasLogicCenter shareLogicCenter] setShouldShowVersion:YES];

    // 设置忘记密码渠道
    [[TQOverSeasLogicCenter shareLogicCenter] setForgetPasswordUrl:[[NSUserDefaults standardUserDefaults] valueForKey:@"NDSetForgetPassword"]];
    // 设置vtc绑定状态
    [[TQOverSeasLogicCenter shareLogicCenter] setVtcBindType:UNI_VTCBind_do_suggest];
    // 设置默认三方列表
    [[TQOverSeasLogicCenter shareLogicCenter] setThirdPlatformDefaultArray:@[]];

    [[TQOverSeasLogicCenter shareLogicCenter] setThirdLoginGuideTime:YES];
    //是否展示注销按钮
    [[TQOverSeasLogicCenter shareLogicCenter] setShowCancllationView:YES];
    //是否外部打开注销web页面
    [[TQOverSeasLogicCenter shareLogicCenter] setExternalOpenCancllationWebView:[[[NSUserDefaults standardUserDefaults] valueForKey:@"NDShouldExternalOpenCancllationWebView"] doubleValue]];

    //是否上报FB头像URL
    [[TQOverSeasLogicCenter shareLogicCenter] setshouldSaveFBPhotoPic:YES];

    // 增加统计SDK
    BOOL enble = [[[NSUserDefaults standardUserDefaults] valueForKey:@"NDAnalyticCollectCrash"] boolValue];

    //新游客登录
    [[TQOverSeasLogicCenter shareLogicCenter] setUserNewTouristLogin:YES];

    //使用相册保存账号密码
    [[TQOverSeasLogicCenter shareLogicCenter] setUseAlbumSaveAccountAndPassword:YES];

    //查询可登录的三方
    [[TQOverSeasLogicCenter shareLogicCenter] setQueryLoginThirdPlatformList:YES];
    //查询登录成功后绑定的三方
    [[TQOverSeasLogicCenter shareLogicCenter] setQueryBuondThirdPlatformList:YES];

    /// 是否调用verity/Token接口
    [[TQOverSeasLogicCenter shareLogicCenter] isGetUnionToken:YES];

    NSString *analy_appid = @"21debdfe66b8423794ef4e2ed5f313cd";
    NSString *analy_appkey = @"dfhs145492rhd#lhdc..gg";

    [[TQOverSeasLogicCenter shareLogicCenter] analyticInitAppid:analy_appid
                                                         appkey:analy_appkey
                                           externalSdkSessionId:nil
                                                          extra:nil
                                                        isInner:network
                                                  isuploadCrash:enble
                                                      isForeign:YES];

    //ndsdk日志
    NSString *fileName =[[NSUserDefaults standardUserDefaults] valueForKey:@"nd_sdk_log_fileName"];
    NSInteger type =[[NSUserDefaults standardUserDefaults] integerForKey:@"nd_sdk_log_type"];
    [[TQOverSeasLogicCenter shareLogicCenter] setLogType:(int)type logDir:nil logFileName:fileName];
}

//初始化
extern void NDSSDK_Init(const char *result) {
    int a = 0;
    NSLog(@"开始初始化");
    //设置代理，
    [[TQOverSeasLogicCenter shareLogicCenter] setDelegate:[OverSeasCallbackHandle callbackHandle]];
    //加载配置信息
    loadSetupInfo();

    TQOverSeasInitModel *model = [TQOverSeasInitModel new];
    model.platformId = SDK_PLATFORM_ID_ND;
    model.domainName = [DemoUtil networkType]==0?@"https://hw-unisdk.99.com":@"http://192.168.182.146:8087";

    TQOverSeasChannelInitModel *channelModel = [TQOverSeasChannelInitModel new];
    NSString *appid = [[NSUserDefaults standardUserDefaults] objectForKey:@"NDSwitchAppid"];
    if (!appid) {
        appid = @"301001";
    }
    NSDictionary *dict = @{@"301001":@"76e69652c2e7653b51e487ab8837cd5bc9a5e235eef8d808",
                           @"301018":@"3dcee76be7351ad60b76635541b4e950bbaf0c16b8c0519e",
                           @"302002":@"3bc89652c2e7353b65e478ab8786cd5bc9a5e235e9fcda56",
                           @"302004":@"bbc89652c2e4353c65e599ab8786cd5bc9a5e235e9fada56",
                           @"302006":@"abc896c2c2e7353c65e599ab8786cd5bc9a5e235e9fada56",
                           @"302008":@"bcc296c2c2e7353c65e599ab8786cd5bc9a5e235e9fada56",
                           @"303001":@"ddcee86be7351ad60b766355415be950bbaa0c65b8c05195",
                           @"303101":@"dc04b6f64104f3235a9b45778a20344abbaf0016b8c051ac",
                           @"303103":@"9ece504285414dc86c22e13711cfd0e4",
                           @"303202":@"d9e71c0f6190a12cb0a31b4f82fecaf8",
                           @"303005":@"8cdc8e16daa16d07edb889618e67b027",
    };
    channelModel.appKey = dict[appid];
    channelModel.appId = [appid intValue];

    channelModel.testUrl = (int)[DemoUtil networkType];

    model.channelInitParam = channelModel;

    model.yDunAppid = @"97504ee0e3554f27880637e4946e9925";

    TQOverSeasTongDunAppModel *tongDunModel = [TQOverSeasTongDunAppModel new];
    tongDunModel.partner = @"4056444d0f5349ee8c2f2e3c40039f95";
    tongDunModel.profileUrl = @"https://usfp.tongdun.net/ios3_8/profile.json";
    tongDunModel.allowd = @"allowd";
    tongDunModel.timeLimit = @"8";
    model.tongDunInitParam = tongDunModel;

    [[TQOverSeasLogicCenter shareLogicCenter] Init:model];
    
}
extern void NDSSDK_OpenPrivacyAgreement(id result) {

    
}
extern void NDSSDK_SetDelegate(MessageResultHandler result1,MessageResultHandler result2,MessageResultHandler result3,MessageResultHandler result4,MessageResultHandler result5,MessageResultHandler result6,MessageResultHandler result7) {
    
    [OverSeasCallbackHandle callbackHandle].onInitResultHandler= result1;
    [OverSeasCallbackHandle callbackHandle].onLoginResultHandler= result2;
    [OverSeasCallbackHandle callbackHandle].onLogoutResultHandler = result3;
    [OverSeasCallbackHandle callbackHandle].onRechaegeResultHandler = result4;
    [OverSeasCallbackHandle callbackHandle].onExitResultHandler = result5;
    [OverSeasCallbackHandle callbackHandle].onExtraDataResultHandler = result6;
    [OverSeasCallbackHandle callbackHandle].onBuiltinActionResultHandler = result7;
}
extern void NDSSDK_Login(int result) {
    /*
    public const int LoginType_Normal = 0;
    public const int LoginType_SwitchAccount = 1;
    public const int LoginType_Wechat = 10;
    public const int LoginType_QQ = 20;
     */
    
    if (result == 0) {
        //普通登录
        [[TQOverSeasLogicCenter shareLogicCenter] Login:nil];
    }else if (result == 10){
        //微信登录
    }else if (result == 20){
        //QQ登录
    }else if (result == 1){
        //切换账号
        [[TQOverSeasLogicCenter shareLogicCenter] SwitchAccount:nil];
    }
    
    NSLog(@"开始登录");
}
extern void NDSSDK_Logout(const char result) {

    NSLog(@"退出登录");
    [[TQOverSeasLogicCenter shareLogicCenter] LoginOut:nil];
    
}
extern void NDSSDK_Recharge(const char result) {
    
    NSLog(@"充值");
    RechargeViewController *controller = [RechargeViewController new];
    NSMutableArray *rechargeInfoArray = [NSMutableArray array];

    RechargeInfo *info = [RechargeInfo new];
    info.rechargeKey = @"productId";
    info.rechargeName = @"商品id";
    info.rechargeValue = @"2197";
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"productName";
    info.rechargeName = @"商品名称";
    info.rechargeValue = @"100点卡";
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"productDesc";
    info.rechargeName = @"商品描述";
    info.rechargeValue = @"点卡";
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"rechargeGameServerId";
    info.rechargeName = @"服务器id";
    info.rechargeValue = @"22";
    info.rechargeType = RechargeKeyTypeNumber;
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"productPrice";
    info.rechargeName = @"商品价格（分）";
    info.rechargeValue = @"99";
    info.rechargeType = RechargeKeyTypeNumber;
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"totalCount";
    info.rechargeName = @"商品数量";
    info.rechargeValue = @"1";
    info.rechargeType = RechargeKeyTypeNumber;
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"totalAmount";
    info.rechargeName = @"商品总价（分）";
    info.rechargeValue = @"99";
    info.rechargeType = RechargeKeyTypeNumber;
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"roleId";
    info.rechargeName = @"角色id";
    info.rechargeValue = @"1";
    [rechargeInfoArray addObject:info];

    info = [RechargeInfo new];
    info.rechargeKey = @"customData";
    info.rechargeName = @"自定义数据";
    info.rechargeValue = @"customData";
    [rechargeInfoArray addObject:info];


    controller.rechargeInfoArray = rechargeInfoArray;
    controller.rechargeEventBlock = ^(NSArray<RechargeInfo *> * _Nonnull rechargeInfoArray) {
        TQOverSeasRechargeModel *model = [TQOverSeasRechargeModel new];
        [rechargeInfoArray enumerateObjectsUsingBlock:^(RechargeInfo * _Nonnull obj, NSUInteger idx, BOOL * _Nonnull stop) {

            if (obj.rechargeType == RechargeKeyTypeNumber) {
                if ([obj.rechargeKey isEqualToString:@"productPrice"]) {
                    model.productPrice = [obj.rechargeValue integerValue];
                }
                if ([obj.rechargeKey isEqualToString:@"rechargeGameServerId"]) {
                    model.rechargeGameServerId = [obj.rechargeValue integerValue];
                }
                if ([obj.rechargeKey isEqualToString:@"totalCount"]) {
                    model.totalCount = [obj.rechargeValue integerValue];
                }
                if ([obj.rechargeKey isEqualToString:@"totalAmount"]) {
                    model.totalAmount = [obj.rechargeValue integerValue];
                }

            }
            else {
                [model setValue:obj.rechargeValue forKey:obj.rechargeKey];
            }
        }];

        CFUUIDRef theUUID = CFUUIDCreate(NULL);
        CFStringRef guid = CFUUIDCreateString(NULL, theUUID);
        CFRelease(theUUID);
        NSString *uuidString = [((NSString *)CFBridgingRelease(guid)) stringByReplacingOccurrencesOfString:@"-" withString:@""];
        //发起购买
        uuidString = [uuidString lowercaseString];//必填
        model.outOrderId = uuidString;

        TQOverSeasChannelRechargeModel *channelModel = [TQOverSeasChannelRechargeModel new];
        model.channelRechargeParam = channelModel;

        [[TQOverSeasLogicCenter shareLogicCenter] Recharge:model];
    };
//    [self.navigationController pushViewController:controller animated:true];

    [[UIApplication sharedApplication].keyWindow.rootViewController presentViewController:controller animated:NO completion:nil];
    
}
extern void NDSSDK_Close(const char result) {
    
    int a = 0;
    
}

extern void NDSSDK_TrackEvent(const char result) {
    
    int a = 0;
    
}

extern void NDSSDK_CustomEvent(const char result) {
    
    int a = 0;
    
}


#ifdef __cplusplus
}
#endif
