//
//  RechargeViewController.m
//  TQHWDemo
//
//  Created by 陈晋添 on 2020/4/8.
//  Copyright © 2020 nd. All rights reserved.
//

#import "RechargeViewController.h"

@implementation RechargeInfo

@end
@interface RechargeViewController ()<UITableViewDelegate,UITableViewDataSource,UITextFieldDelegate>
@property (nonatomic, strong) UITableView *tableView;
@end
@interface RechargeViewController ()

@end

@implementation RechargeViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    [self.view addSubview:self.tableView];
}

- (NSInteger)numberOfSectionsInTableView:(UITableView *)tableView {
    return 1;
}

- (NSInteger)tableView:(UITableView *)tableView numberOfRowsInSection:(NSInteger)section {
    return self.rechargeInfoArray.count;
}

- (UITableViewCell *)tableView:(UITableView *)tableView cellForRowAtIndexPath:(NSIndexPath *)indexPath {
    UITableViewCell *cell = [UITableViewCell new];
    cell.textLabel.text = self.rechargeInfoArray[indexPath.row].rechargeName;
    
    UITextField *textField = [[UITextField alloc] initWithFrame:CGRectMake(180, 0, tableView.frame.size.width-180, 44)];
    textField.delegate = self;
    textField.text = self.rechargeInfoArray[indexPath.row].rechargeValue;
    textField.tag = indexPath.row;
    [cell addSubview:textField];
    return cell;
}

- (void)tableView:(UITableView *)tableView didSelectRowAtIndexPath:(NSIndexPath *)indexPath {
    [tableView deselectRowAtIndexPath:indexPath animated:true];
}

- (UIView *)tableView:(UITableView *)tableView viewForFooterInSection:(NSInteger)section {
    UIView *footerView = [[UIView alloc] initWithFrame:CGRectMake(0, 0, CGRectGetWidth(tableView.frame), 60)];
    UIButton *button = [UIButton buttonWithType:0];
    [button setTitle:@"确定" forState:UIControlStateNormal];
    [button setBackgroundColor:[UIColor orangeColor]];
    button.frame = CGRectMake(0, 0, 120, 40);
    [button addTarget:self action:@selector(rechargeEvent) forControlEvents:UIControlEventTouchUpInside];
    button.center = CGPointMake(CGRectGetWidth(tableView.frame)/2.0, 40);
    [footerView addSubview:button];
    return footerView;
}

- (CGFloat)tableView:(UITableView *)tableView heightForRowAtIndexPath:(NSIndexPath *)indexPath {
    return 44;
}

- (CGFloat)tableView:(UITableView *)tableView heightForFooterInSection:(NSInteger)section {
    return 60;
}

- (void)rechargeEvent {
    if (self.rechargeEventBlock) {
        self.rechargeEventBlock(self.rechargeInfoArray);
    }
    [self.navigationController popViewControllerAnimated:true];
}

- (void)textFieldDidEndEditing:(UITextField *)textField {
    self.rechargeInfoArray[textField.tag].rechargeValue = textField.text;
}

- (void)touchesBegan:(NSSet<UITouch *> *)touches withEvent:(UIEvent *)event {
    [super touchesBegan:touches withEvent:event];
    
    [self.view endEditing:true];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField {
    [self.view endEditing:true];
    return true;
}

- (UITableView *)tableView {
    if (!_tableView) {
        _tableView = [[UITableView alloc] initWithFrame:self.view.bounds style:UITableViewStyleGrouped];
        _tableView.delegate = self;
        _tableView.dataSource = self;
    }
    return _tableView;
}

-(BOOL)hidesBottomBarWhenPushed {
    
    return YES;
}

@end
