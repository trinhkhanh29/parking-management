﻿using ParkingManagement.Models;
using System;
using ParkingManagement.BLL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace ParkingManagement.GUI.Forms
{
    public partial class frmCreateUser : Form
    {
        private readonly parkingAreaBLL areaBLL = new parkingAreaBLL();
        private roleBLL roleBLL = new roleBLL();
        private userBLL userBLL = new userBLL();

        public frmCreateUser()
        {
            InitializeComponent();
            LoadParkingAreas();
            LoadRole();
        }

        private void LoadParkingAreas()
        {
            try
            {
                List<parkingAreaModel> areas = areaBLL.GetAllParkingAreas();

                if (areas == null || areas.Count == 0)
                {
                    MessageBox.Show("Không có khu vực nào trong cơ sở dữ liệu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    kAreaCbb.DataSource = areas;
                    kAreaCbb.DisplayMember = "AreaName";
                    kAreaCbb.ValueMember = "Id";
                    kAreaCbb.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách khu vực: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRole()
        {
            try
            {
                List<roleModel> roles = roleBLL.GetAllRoleAreas();

                if (roles != null && roles.Count > 0)
                {
                    kTypeAccCbb.DataSource = roles;
                    kTypeAccCbb.DisplayMember = "RoleName";
                    kTypeAccCbb.ValueMember = "Id";
                    kTypeAccCbb.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("Không có quyền nào trong cơ sở dữ liệu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải danh sách quyền: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            kFullNameTb.Clear();
            kPhoneTb.Clear();
            kEmailTb.Clear();
            kAddressTb.Clear();
            kPasswordTb.Clear(); // clear password
            kSexCbb.SelectedIndex = -1;
            kDtBirthday.Value = DateTime.Now;
            kTypeAccCbb.SelectedIndex = -1;
            kAreaCbb.SelectedIndex = -1;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Convert byte to hex string
                }
                return builder.ToString();
            }
        }

        private void kSaveAddUser_Click(object sender, EventArgs e)
        {
            try
            {
                string fullName = kFullNameTb.Text.Trim();
                string phoneNumber = kPhoneTb.Text.Trim();
                string email = kEmailTb.Text.Trim();
                string address = kAddressTb.Text.Trim();
                string rawPassword = kPasswordTb.Text.Trim(); // new line
                string gender = kSexCbb.SelectedItem?.ToString();
                DateTime? dateOfBirth = kDtBirthday.Value;
                bool status = true;

                if (string.IsNullOrWhiteSpace(rawPassword))
                {
                    MessageBox.Show("Vui lòng nhập mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (rawPassword.Length < 6)
                {
                    MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                else if (!rawPassword.Any(char.IsDigit) || !rawPassword.Any(char.IsLetter))
                {
                    MessageBox.Show("Mật khẩu phải chứa cả chữ và số!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string hashedPassword = HashPassword(rawPassword);

                Guid? roleId = (kTypeAccCbb.SelectedValue is Guid guidRole) ? guidRole : (Guid?)null;
                Guid? areaId = kAreaCbb.SelectedValue != null ? Guid.Parse(kAreaCbb.SelectedValue.ToString()) : (Guid?)null;

                if (roleId == null)
                {
                    MessageBox.Show("Vui lòng chọn vai trò!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (areaId == null || areaId == Guid.Empty)
                {
                    MessageBox.Show("Area ID không hợp lệ! Debug: " + kAreaCbb.SelectedValue, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string codePrefix = "USER";
                string selectedRole = kTypeAccCbb.Text.Trim().ToLower();

                if (selectedRole.Contains("admin")) codePrefix = "AD";
                else if (selectedRole.Contains("customer")) codePrefix = "KH";
                else if (selectedRole.Contains("staff")) codePrefix = "NV";

                string timeStamp = DateTime.Now.ToString("yyMMddHHmm");
                string code = codePrefix + timeStamp;

                createUser newUser = new createUser
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    FullName = fullName,
                    PhoneNumber = phoneNumber,
                    Email = email,
                    Address = address,
                    Gender = gender,
                    DateOfBirth = dateOfBirth,
                    RoleId = roleId,
                    AreaId = areaId,
                    Status = status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Password = hashedPassword // add password
                };

                userBLL userService = new userBLL();
                userService.AddUser(newUser);

                MessageBox.Show("Thêm người dùng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu user: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void kryptonPanel1_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}