using Click_.Controls;
using System.Windows.Forms;

namespace Click_
{
    partial class Click
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Click));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.messengersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.desctopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.skypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.telegramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webVersionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webSkypeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webWhatsAppToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.webTelegramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bindsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lastUsedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mostNewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orderMessengersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.messengersToolStripMenuItem,
            this.bindsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 92);
            // 
            // messengersToolStripMenuItem
            // 
            this.messengersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.desctopToolStripMenuItem,
            this.webVersionToolStripMenuItem});
            this.messengersToolStripMenuItem.Name = "messengersToolStripMenuItem";
            this.messengersToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.messengersToolStripMenuItem.Text = "Messengers";
            // 
            // desctopToolStripMenuItem
            // 
            this.desctopToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.skypeToolStripMenuItem,
            this.telegramToolStripMenuItem});
            this.desctopToolStripMenuItem.Name = "desctopToolStripMenuItem";
            this.desctopToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.desctopToolStripMenuItem.Text = "Desktop";
            // 
            // skypeToolStripMenuItem
            // 
            this.skypeToolStripMenuItem.CheckOnClick = true;
            this.skypeToolStripMenuItem.Name = "skypeToolStripMenuItem";
            this.skypeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.skypeToolStripMenuItem.Text = "Skype";
            this.skypeToolStripMenuItem.Click += new System.EventHandler(this.skypeToolStripMenuItem_Click);
            // 
            // telegramToolStripMenuItem
            // 
            this.telegramToolStripMenuItem.CheckOnClick = true;
            this.telegramToolStripMenuItem.Name = "telegramToolStripMenuItem";
            this.telegramToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.telegramToolStripMenuItem.Text = "Telegram";
            this.telegramToolStripMenuItem.Click += new System.EventHandler(this.telegramToolStripMenuItem_Click);
            // 
            // webVersionToolStripMenuItem
            // 
            this.webVersionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.webSkypeToolStripMenuItem,
            this.webWhatsAppToolStripMenuItem,
            this.webTelegramToolStripMenuItem});
            this.webVersionToolStripMenuItem.Name = "webVersionToolStripMenuItem";
            this.webVersionToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.webVersionToolStripMenuItem.Text = "Web Version";
            // 
            // webSkypeToolStripMenuItem
            // 
            this.webSkypeToolStripMenuItem.CheckOnClick = true;
            this.webSkypeToolStripMenuItem.Name = "webSkypeToolStripMenuItem";
            this.webSkypeToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.webSkypeToolStripMenuItem.Text = "Skype";
            this.webSkypeToolStripMenuItem.Click += new System.EventHandler(this.webSkypeToolStripMenuItem_Click);
            // 
            // webWhatsAppToolStripMenuItem
            // 
            this.webWhatsAppToolStripMenuItem.CheckOnClick = true;
            this.webWhatsAppToolStripMenuItem.Name = "webWhatsAppToolStripMenuItem";
            this.webWhatsAppToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.webWhatsAppToolStripMenuItem.Text = "WhatsApp";
            this.webWhatsAppToolStripMenuItem.Click += new System.EventHandler(this.webWhatsAppToolStripMenuItem_Click);
            // 
            // webTelegramToolStripMenuItem
            // 
            this.webTelegramToolStripMenuItem.CheckOnClick = true;
            this.webTelegramToolStripMenuItem.Name = "webTelegramToolStripMenuItem";
            this.webTelegramToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.webTelegramToolStripMenuItem.Text = "Telegram";
            this.webTelegramToolStripMenuItem.Click += new System.EventHandler(this.webTelegramToolStripMenuItem_Click);
            // 
            // bindsToolStripMenuItem
            // 
            this.bindsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lastUsedToolStripMenuItem,
            this.mostNewToolStripMenuItem,
            this.orderMessengersToolStripMenuItem,
            this.updateToolStripMenuItem});
            this.bindsToolStripMenuItem.Name = "bindsToolStripMenuItem";
            this.bindsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.bindsToolStripMenuItem.Text = "Bindings";
            // 
            // lastUsedToolStripMenuItem
            // 
            this.lastUsedToolStripMenuItem.CheckOnClick = true;
            this.lastUsedToolStripMenuItem.Name = "lastUsedToolStripMenuItem";
            this.lastUsedToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.lastUsedToolStripMenuItem.Text = "Last Used";
            this.lastUsedToolStripMenuItem.Click += new System.EventHandler(this.activityToolStripMenuItem_Click);
            // 
            // mostNewToolStripMenuItem
            // 
            this.mostNewToolStripMenuItem.CheckOnClick = true;
            this.mostNewToolStripMenuItem.Name = "mostNewToolStripMenuItem";
            this.mostNewToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.mostNewToolStripMenuItem.Text = "Most New Messages";
            this.mostNewToolStripMenuItem.Click += new System.EventHandler(this.lastActiveMessengerToolStripMenuItem_Click);
            // 
            // orderMessengersToolStripMenuItem
            // 
            this.orderMessengersToolStripMenuItem.CheckOnClick = true;
            this.orderMessengersToolStripMenuItem.Name = "orderMessengersToolStripMenuItem";
            this.orderMessengersToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.orderMessengersToolStripMenuItem.Text = "Order";
            this.orderMessengersToolStripMenuItem.Click += new System.EventHandler(this.orderMessengersToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipText = "Click!";
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            this.updateToolStripMenuItem.Text = "Update";
            // 
            // Click
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.ControlBox = false;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Click";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem messengersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bindsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lastUsedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mostNewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orderMessengersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem desctopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem skypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem telegramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem webVersionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem webSkypeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem webWhatsAppToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem webTelegramToolStripMenuItem;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private ToolStripMenuItem updateToolStripMenuItem;
    }
}

