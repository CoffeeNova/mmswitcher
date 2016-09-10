namespace Click_.Controls
{
    partial class BindControl
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Обязательный метод для поддержки конструктора - не изменяйте 
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.modComboBox = new System.Windows.Forms.ComboBox();
            this.bindComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // modComboBox
            // 
            this.modComboBox.DisplayMember = "ModTitle";
            this.modComboBox.FormattingEnabled = true;
            this.modComboBox.Location = new System.Drawing.Point(4, 4);
            this.modComboBox.Name = "modComboBox";
            this.modComboBox.Size = new System.Drawing.Size(100, 21);
            this.modComboBox.TabIndex = 0;
            this.modComboBox.ValueMember = "Mod";
            // 
            // bindComboBox
            // 
            this.bindComboBox.DisplayMember = "BindTitle";
            this.bindComboBox.FormattingEnabled = true;
            this.bindComboBox.Location = new System.Drawing.Point(110, 4);
            this.bindComboBox.Name = "bindComboBox";
            this.bindComboBox.Size = new System.Drawing.Size(86, 21);
            this.bindComboBox.TabIndex = 1;
            this.bindComboBox.ValueMember = "Bind";
            // 
            // BindControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.bindComboBox);
            this.Controls.Add(this.modComboBox);
            this.Name = "BindControl";
            this.Size = new System.Drawing.Size(199, 91);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox modComboBox;
        private System.Windows.Forms.ComboBox bindComboBox;
    }
}
