//
//  RechargeViewController.h
//  TQHWDemo
//
//  Created by 陈晋添 on 2020/4/8.
//  Copyright © 2020 nd. All rights reserved.
//

#import <UIKit/UIKit.h>

NS_ASSUME_NONNULL_BEGIN

typedef NS_ENUM(NSUInteger, RechargeKeyType) {
    RechargeKeyTypeString,
    RechargeKeyTypeNumber,
};

@interface RechargeInfo : NSObject
@property (nonatomic, strong) NSString *rechargeKey;
@property (nonatomic, strong) NSString *rechargeName;
@property (nonatomic, strong) NSString *rechargeValue;
@property (nonatomic, assign) RechargeKeyType rechargeType;
@end

@interface RechargeViewController : UIViewController

@property (nonatomic, strong) NSArray<RechargeInfo *> *rechargeInfoArray;

@property (nonatomic, strong) void(^rechargeEventBlock)(NSArray<RechargeInfo *> *rechargeInfoArray);

@end

NS_ASSUME_NONNULL_END
