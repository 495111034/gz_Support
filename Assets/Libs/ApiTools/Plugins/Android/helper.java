package com.game.apitools;

import android.app.Activity;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Intent;
import android.graphics.BitmapFactory;
import android.os.Build;
import android.os.Process;
import androidx.core.app.NotificationCompat;
import android.util.Log;
import com.unity3d.player.UnityPlayer;

import android.content.Context;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.telephony.TelephonyManager;
import java.lang.reflect.Field;
import java.lang.reflect.Modifier;

import android.database.Cursor;
import android.app.DownloadManager;
import android.content.BroadcastReceiver;
import android.net.Uri;
import java.util.ArrayList;


import android.app.DownloadManager;
import android.app.Notification;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.os.Bundle;
//import com.google.android.material.floatingactionbutton.FloatingActionButton;
//import com.google.android.material.snackbar.Snackbar;

import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;
import android.os.IBinder;
import android.util.Log;
import android.view.View;
import android.view.Menu;
import android.view.MenuItem;
import java.util.ArrayList;

import java.io.File;
import android.util.Log;
import java.io.BufferedReader;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStreamReader;



import android.os.Environment;
import android.os.StatFs;



public class helper {
    public Activity currentActivity;
    private static helper _instance;
    private final static String TAG="LogActivity";
    private final static int mProgressNotificationId = 1; //用于下载进度显示的通知栏通知ID
    private final static String NOTIFICATION_ID = "nd";
    private final static String NOTIFICATION_NAME = "nd_long";
    private NotificationManager mManager;
    private NotificationCompat.Builder mBuilder;


    public helper() {
    }

    public static helper Instance() {
        if (_instance == null) {
            _instance = new helper();
        }

        return _instance;
    }

    public void Init(Activity content, String MainGameObjectName) {
        this.currentActivity = content;
    }

    public String GetCPU() {
        String cpu = "";
        try {
            cpu = Build.HARDWARE;
        } catch (Exception e) {

        }
        return cpu;
    }

    public void RestartAndroidApp() {
        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                (new Thread() {
                    public void run() {
                        Object localObject = UnityPlayer.currentActivity.getBaseContext().getPackageManager().getLaunchIntentForPackage(helper.this.currentActivity.getPackageName());
                        ((Intent)localObject).addFlags(67108864);
                        UnityPlayer.currentActivity.startActivity((Intent)localObject);
                        Process.killProcess(Process.myPid());
                    }
                }).start();
                UnityPlayer.currentActivity.finish();
            }
        });
    }

    public void ProgressNotify(String title, String text, int progressCurrent, int progressMax)
    {
		if(mManager == null)
		{
			mManager = (NotificationManager) currentActivity.getSystemService(currentActivity.NOTIFICATION_SERVICE);
			// 适配8.0及以上 创建渠道
			if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
				NotificationChannel channel = new NotificationChannel(NOTIFICATION_ID, NOTIFICATION_NAME, NotificationManager.IMPORTANCE_DEFAULT);
				mManager.createNotificationChannel(channel);
			}
		}

		if(mBuilder == null) {
			mBuilder = new NotificationCompat.Builder(currentActivity, NOTIFICATION_ID);
		}

		mBuilder.setContentTitle(title);
		mBuilder.setContentText(text);
		mBuilder.setOnlyAlertOnce(true);
		//getResources().getIdentifier("app_icon", "drawable", getPackageName())
		int app_icon_id = currentActivity.getResources().getIdentifier("app_icon", "mipmap", currentActivity.getPackageName());
		mBuilder.setSmallIcon(app_icon_id);
		mBuilder.setLargeIcon(BitmapFactory.decodeResource(currentActivity.getResources(), app_icon_id));
		// 第3个参数indeterminate，false表示确定的进度，比如100，true表示不确定的进度，会一直显示进度动画，直到更新状态下载完成，或删除通知
		mBuilder.setProgress(progressMax, progressCurrent, false);

		mManager.notify(mProgressNotificationId, mBuilder.build());
            
    }

    public void CancelProgressNotify(){
		if(mManager != null) {
			mManager.cancel(mProgressNotificationId);//删除通知栏图标
		}
    }	
	
	public String GetNetworkTypeName()
	{
		try
		{
			Context context = UnityPlayer.currentActivity.getBaseContext();			
			ConnectivityManager connectMgr = (ConnectivityManager) context.getSystemService(Context.CONNECTIVITY_SERVICE);
			if(connectMgr == null)
			{
				return "nomgr";
			}			
			NetworkInfo info = connectMgr.getActiveNetworkInfo();
			if (info == null)
			{
				return "noinfo";
			}			
			return info.getTypeName() + "/" + info.getSubtypeName();
		}catch(Exception e)
		{
			return "exception";
		}
	}	
	
	public void StartDownload(String url, String savePath)
	{		
		try
		{	
			Log.i("DownloadManager",url);
			Context context = UnityPlayer.currentActivity.getBaseContext();	
			DownloadManager.Request request = new DownloadManager.Request(Uri.parse(url));
			request.setNotificationVisibility(DownloadManager.Request.VISIBILITY_HIDDEN);
			request.setDestinationUri(Uri.parse(savePath));
			//
			DownloadManager manager = (DownloadManager)context.getSystemService(Context.DOWNLOAD_SERVICE);
			long id = manager.enqueue(request);
			Log.i("DownloadManager","enqueue, id=" + id);
			//
		}catch(Exception e2)
		{
			e2.printStackTrace();
		}		
	}
	
	Uri report_url;
	public void SetReportURL(String url)
	{
		Log.i("DownloadManager", "report_url=" + url);
		report_url = Uri.parse(url);// + "?REQBODY=";		
	}
	public void StartReport(String req_body)
	{		
		try
		{	
			//Log.d("DownloadManager", "StartReport");
			Context context = UnityPlayer.currentActivity.getBaseContext();	
			DownloadManager.Request request = new DownloadManager.Request(report_url);//Uri.parse(report_url + req_body)
			request.setNotificationVisibility(DownloadManager.Request.VISIBILITY_HIDDEN);
			request.addRequestHeader("REQBODY", req_body);
			DownloadManager manager = (DownloadManager)context.getSystemService(Context.DOWNLOAD_SERVICE);
			long id = manager.enqueue(request);
			Log.d("DownloadManager","StartReport, id=" + id);
		}catch(Exception e2)
		{
			e2.printStackTrace();
		}		
		
		CheckReports();
	}	
	
	public void CheckReports()
	{
		try
		{	
			Context context = UnityPlayer.currentActivity.getBaseContext();
			DownloadManager manager = (DownloadManager)context.getSystemService(Context.DOWNLOAD_SERVICE);
			Cursor cursor = manager.query(new DownloadManager.Query());
			long del = -1;
			while (cursor.moveToNext())
			{
				long id = cursor.getLong(cursor.getColumnIndex(DownloadManager.COLUMN_ID));
				int status = cursor.getInt(cursor.getColumnIndex(DownloadManager.COLUMN_STATUS));
				//Log.d("DownloadManager", id + " -> status=" + status);
				if (status == DownloadManager.STATUS_FAILED)
				{				
					Log.e("DownloadManager", "CheckReports " + id + " -> STATUS_FAILED");
					del = id;					
					break;
				}				
				if (status == DownloadManager.STATUS_SUCCESSFUL)
				{
					String savePath = cursor.getString(cursor.getColumnIndex(DownloadManager.COLUMN_LOCAL_URI));
					if(savePath == null || savePath.length() == 0 || !((new File(savePath)).exists()))
					{
						Log.d("DownloadManager", "CheckReports " + id + " -> STATUS_SUCCESSFUL");
						del = id;	
						break;
					}
				}				
			}			
			cursor.close();
			//			
			if(del != -1)
			{
				Log.d("DownloadManager", "CheckReports " + del + " -> remove");		
				manager.remove(del);
			}
			//
		}catch(Exception e2)
		{
			e2.printStackTrace();
		}		
	}
	
	public boolean IsEmulator()
    {
        return isEmulatorFromAbi() || isEmulatorFromCpu();
    }
	
	private boolean isEmulatorFromAbi()
	{
		try
		{
			String os_cpuabi = new BufferedReader(new InputStreamReader(Runtime.getRuntime().exec("getprop ro.product.cpu.abi").getInputStream())).readLine();
			if (os_cpuabi.contains("x86"))
			{
				return true;
			}
		}
		catch (IOException e)
		{
			e.printStackTrace();
		}
		return false;
	}
	private boolean isEmulatorFromCpu()
	{
		String cpu = "";
		try
		{
			String str1 = "/proc/cpuinfo";
			FileReader fr = new FileReader(str1);
			BufferedReader localBufferedReader = new BufferedReader(fr, 8192);
			String line = null;
			while ((line = localBufferedReader.readLine()) != null)
			{
				if (line.toLowerCase().indexOf("hardware") != -1)
				{
					cpu = line;
					break;
				}
			}
			localBufferedReader.close();
		}
		catch (Exception e)
		{
			Log.d("isEmulatorFromCpu", e.getMessage());
		}
		if (cpu != null)
		{
			if ((cpu.toLowerCase().contains("intel") || cpu.toLowerCase().contains("amd")))
			{
				return true;
			}
		}
		return false;
	}

	public String getInternalStorageSpace(String dir) 
	{
		String ret = "";
		try
		{
			//File path = Environment.getDataDirectory();
			//String dir = path.getPath();
			StatFs statFs = new StatFs(dir);
			long blockSize = statFs.getBlockSizeLong();
			long totalBlocks = statFs.getBlockCountLong();
			long availableBlocks = statFs.getAvailableBlocksLong(); 
			ret = "disk space=" + (blockSize * availableBlocks) + "/" + (blockSize * totalBlocks) + ", dir=" + dir;
		}catch(Exception e)
		{
			ret = e.getMessage();
		}
		return ret;
	}
}









