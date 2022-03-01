package melelauncherGUI;

import org.eclipse.swt.SWT;
import org.eclipse.swt.widgets.Display;
import org.eclipse.swt.widgets.Shell;
import org.eclipse.swt.widgets.Text;
import org.eclipse.swt.widgets.Menu;
import org.eclipse.swt.widgets.Button;
import org.eclipse.swt.widgets.MenuItem;
import org.eclipse.swt.widgets.MessageBox;
import org.eclipse.swt.events.MouseAdapter;
import org.eclipse.swt.events.MouseEvent;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;

import java.util.*;

import javax.swing.JFileChooser;

import java.io.*;
import org.eclipse.swt.custom.CLabel;
import org.eclipse.wb.swt.SWTResourceManager;
import org.eclipse.swt.widgets.ProgressBar;

public class LauncherForm {

	protected Shell shlTestingApp;
	private Text localeEntry;
	public static String gamePath;
	private Text fntSizeEntry;
	private Text gamepathEntry;

	/**
	 * Launch the application.
	 * @param args
	 */
	public static void main(String[] args) {
		try {
			LauncherForm window = new LauncherForm();
			window.open();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	/**
	 * Open the window.
	 */
	public void open() {
		Display display = Display.getDefault();
		createContents();
		shlTestingApp.open();
		shlTestingApp.layout();
		while (!shlTestingApp.isDisposed()) {
			if (!display.readAndDispatch()) {
				display.sleep();
			}
		}
	}

	/**
	 * Create contents of the window.
	 */
	protected void createContents() {
		shlTestingApp = new Shell();
		shlTestingApp.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		shlTestingApp.setImage(SWTResourceManager.getImage(LauncherForm.class, "/icons/full/message_info.png"));
		shlTestingApp.setTouchEnabled(true);
		shlTestingApp.setSize(686, 170);
		shlTestingApp.setText("Legendary Launcher");
		ProgressBar intProg = new ProgressBar(shlTestingApp, SWT.VERTICAL);
		intProg.setForeground(SWTResourceManager.getColor(255, 140, 0));
		intProg.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		intProg.setBounds(647, 10, 14, 113);
		System.out.println();
		
		String legacygamePathme1;
		String legacygamePathme2;
		String legacygamePathme3;
		boolean ForceFeedback;
		
		Button btnME1L = new Button(shlTestingApp, SWT.FLAT);
		btnME1L.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		btnME1L.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		btnME1L.addMouseListener(new MouseAdapter() {
			private boolean ForceFeedback;

			@Override
			public void mouseDown(MouseEvent e) {
				MessageBox messageBox = new MessageBox(shlTestingApp, SWT.ICON_QUESTION | SWT.YES | SWT.NO);
				messageBox.setText("Prompt");
		        messageBox.setMessage("Do you wish to enable Controler Force Feedback?");
		        int buttonID = messageBox.open();
		        switch(buttonID) {
		          case SWT.YES:
		        	  this.ForceFeedback = true;
		        	  break;
		          case SWT.NO:
		        	  this.ForceFeedback = false;
		        	  break;
		          
		        }
		        String locale = localeEntry.getText();
		        String fontsizers = fntSizeEntry.getText();
		       
				gamePath = gamepathEntry.getText();
				String fullpath = gamePath + "\\Game\\ME1\\Binaries\\Win64\\MassEffect1.exe";
				ProcessBuilder meforcefeed = new ProcessBuilder(fullpath , " -NoHomeDir" + " -SeekFreeLoadingPCConsole"+" -locale "+ locale + " -OVERRIDELANGUAGE=" + locale + " -Subtitles " + fontsizers + " -TELEMOPTIN 0");
				ProcessBuilder menoforcefeed = new ProcessBuilder(fullpath , " -NoHomeDir" + " -SeekFreeLoadingPCConsole"+" -locale "+ locale + " -OVERRIDELANGUAGE=" + locale + " -Subtitles" + fontsizers + " -NOFORCEFEEDBACK " + " -TELEMOPTIN 0");

		        try {
		        	if (this.ForceFeedback == true) {
		        		meforcefeed.start();
		        		intProg.setSelection(100);
		        	}
		        	else {
		        		menoforcefeed.start();
		        		intProg.setSelection(100);
		        	}
					
				} catch (IOException e1) {
					
					e1.printStackTrace();
				}
			}
		});
		btnME1L.setBounds(10, 10, 199, 25);
		btnME1L.setText("Mass Effect 1 Legendary");
		
		Button btnME2L = new Button(shlTestingApp, SWT.NONE);
		btnME2L.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		btnME2L.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		btnME2L.addMouseListener(new MouseAdapter() {
			private boolean ForceFeedback;
			@Override
			public void mouseDown(MouseEvent e) {
				MessageBox messageBox = new MessageBox(shlTestingApp, SWT.ICON_QUESTION | SWT.YES | SWT.NO);
				messageBox.setText("Prompt");
		        messageBox.setMessage("Do you wish to enable Controler Force Feedback?");
		        int buttonID = messageBox.open();
		        switch(buttonID) {
		          case SWT.YES:
		        	  this.ForceFeedback = true;
		        	  break;
		          case SWT.NO:
		        	  this.ForceFeedback = false;
		        	  break;
		          
		        }
		        String locale = localeEntry.getText();
		        String fontsizers = fntSizeEntry.getText();
		        
		        gamePath = gamepathEntry.getText();
				String fullpath = gamePath + "\\Game\\ME2\\Binaries\\Win64\\MassEffect2.exe";
				ProcessBuilder meforcefeed = new ProcessBuilder(fullpath , " -NoHomeDir" + " -SeekFreeLoadingPCConsole"+" -locale "+ locale + " -OVERRIDELANGUAGE=" + locale + " -Subtitles " + fontsizers + " -TELEMOPTIN 0");
				ProcessBuilder menoforcefeed = new ProcessBuilder(fullpath , " -NoHomeDir" + " -SeekFreeLoadingPCConsole"+" -locale "+ locale + " -OVERRIDELANGUAGE=" + locale + " -Subtitles" + fontsizers + " -NOFORCEFEEDBACK " + " -TELEMOPTIN 0");

		        try {
		        	if (this.ForceFeedback == true) {
		        		meforcefeed.start();
		        		intProg.setSelection(100);
		        	}
		        	else {
		        		menoforcefeed.start();
		        		intProg.setSelection(100);
		        	}
					
				} catch (IOException e1) {
					
					e1.printStackTrace();
				}
			}
		});
		btnME2L.setText("Mass Effect 2 Legendary");
		btnME2L.setBounds(215, 10, 199, 25);
		
		Button btnME3L = new Button(shlTestingApp, SWT.NONE);
		btnME3L.addMouseListener(new MouseAdapter() {
			private boolean ForceFeedback;
			@Override
			public void mouseDown(MouseEvent e) {
				MessageBox messageBox = new MessageBox(shlTestingApp, SWT.ICON_QUESTION | SWT.YES | SWT.NO);
				messageBox.setText("Prompt");
		        messageBox.setMessage("Do you wish to enable Controler Force Feedback?");
		        int buttonID = messageBox.open();
		        switch(buttonID) {
		          case SWT.YES:
		        	  this.ForceFeedback = true;
		        	  break;
		          case SWT.NO:
		        	  this.ForceFeedback = false;
		        	  break;
		          
		        }
		        String locale = localeEntry.getText();
		        String fontsizers = fntSizeEntry.getText();
		        
		        gamePath = gamepathEntry.getText();
				String fullpath = gamePath + "\\Game\\ME3\\Binaries\\Win64\\MassEffect3.exe";
				ProcessBuilder meforcefeed = new ProcessBuilder(fullpath , " -NoHomeDir" + " -SeekFreeLoadingPCConsole"+" -locale "+ locale + " -language=" + locale + " -Subtitles " + fontsizers + " -TELEMOPTIN 0");
				ProcessBuilder menoforcefeed = new ProcessBuilder(fullpath , " -NoHomeDir" + " -SeekFreeLoadingPCConsole"+" -locale "+ locale + " -language=" + locale + " -Subtitles" + fontsizers + " -NOFORCEFEEDBACK " + " -TELEMOPTIN 0");

		        try {
		        	if (this.ForceFeedback == true) {
		        		meforcefeed.start();
		        		intProg.setSelection(100);
		        	}
		        	else {
		        		menoforcefeed.start();
		        		intProg.setSelection(100);
		        	}
					
				} catch (IOException e1) {
					
					e1.printStackTrace();
				}
			}
		});
		btnME3L.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		btnME3L.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		
		btnME3L.setText("Mass Effect 3 Legendary");
		btnME3L.setBounds(420, 10, 199, 25);
		
		Button btnME1C = new Button(shlTestingApp, SWT.NONE);
		btnME1C.addMouseListener(new MouseAdapter() {
			@Override
			public void mouseDown(MouseEvent e) {
				String locale = localeEntry.getText();
		        String fontsizers = fntSizeEntry.getText();
		        
		        gamePath = gamepathEntry.getText();
				String fullpath = gamePath + "\\Binaries\\Win32\\MassEffect.exe";
				ProcessBuilder meproc = new ProcessBuilder(fullpath);
				

		        try {
		        	
		        	meproc.start();
		        	intProg.setSelection(100);
		        
					
				} catch (IOException e1) {
					
					e1.printStackTrace();
				}
			}
		});
		btnME1C.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		btnME1C.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		btnME1C.setText("Mass Effect 1 Classic");
		btnME1C.setBounds(10, 40, 199, 25);
		
		Button btnME2C = new Button(shlTestingApp, SWT.NONE);
		btnME2C.addMouseListener(new MouseAdapter() {
			@Override
			public void mouseDown(MouseEvent e) {
				String locale = localeEntry.getText();
		        String fontsizers = fntSizeEntry.getText();
		        
		        gamePath = gamepathEntry.getText();
				String fullpath = gamePath + "\\Binaries\\Win32\\MassEffect2.exe";
				ProcessBuilder meproc = new ProcessBuilder(fullpath);
				

		        try {
		        	
		        	meproc.start();
		        	intProg.setSelection(100);
		        
					
				} catch (IOException e1) {
					
					e1.printStackTrace();
				}
			}
		});
		btnME2C.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		btnME2C.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		btnME2C.setText("Mass Effect 2 Classic");
		btnME2C.setBounds(215, 40, 199, 25);
		
		Button btnME3C = new Button(shlTestingApp, SWT.NONE);
		btnME3C.addMouseListener(new MouseAdapter() {
			@Override
			public void mouseDown(MouseEvent e) {
				String locale = localeEntry.getText();
		        String fontsizers = fntSizeEntry.getText();
		        
		        gamePath = gamepathEntry.getText();
				String fullpath = gamePath + "\\Binaries\\Win32\\MassEffect3.exe";
				ProcessBuilder meproc = new ProcessBuilder(fullpath);
				

		        try {
		        	
		        	meproc.start();
		        	intProg.setSelection(100);
		        
					
				} catch (IOException e1) {
					
					e1.printStackTrace();
				}
			}
		});
		btnME3C.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		btnME3C.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		btnME3C.setText("Mass Effect 3 Classic");
		btnME3C.setBounds(420, 40, 199, 25);
		
		localeEntry = new Text(shlTestingApp, SWT.BORDER);
		localeEntry.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		localeEntry.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		localeEntry.setBounds(59, 98, 76, 21);
		
		CLabel localeLabel = new CLabel(shlTestingApp, SWT.NONE);
		localeLabel.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		localeLabel.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		localeLabel.setBounds(10, 98, 47, 21);
		localeLabel.setText("Locale");
		
		CLabel lblFnt = new CLabel(shlTestingApp, SWT.NONE);
		lblFnt.setText("Font Size");
		lblFnt.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		lblFnt.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		lblFnt.setBounds(141, 98, 63, 21);
		
		fntSizeEntry = new Text(shlTestingApp, SWT.BORDER);
		fntSizeEntry.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		fntSizeEntry.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		fntSizeEntry.setBounds(208, 98, 76, 21);
		
		CLabel LBLgamepath = new CLabel(shlTestingApp, SWT.NONE);
		LBLgamepath.setText("Gamepath");
		LBLgamepath.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		LBLgamepath.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		LBLgamepath.setBounds(10, 71, 63, 21);
		
		gamepathEntry = new Text(shlTestingApp, SWT.BORDER);
		gamepathEntry.setForeground(SWTResourceManager.getColor(SWT.COLOR_WHITE));
		gamepathEntry.setBackground(SWTResourceManager.getColor(SWT.COLOR_WIDGET_BORDER));
		gamepathEntry.setBounds(79, 71, 553, 21);
		
		

	}
}
