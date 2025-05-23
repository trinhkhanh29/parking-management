﻿using ParkingManagement.BLL;
using ParkingManagement.DAL.Repositories;
using ParkingManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParkingManagement.GUI.Forms.staff
{
    public partial class frmInOutVehicle : Form
    {
        private vehicleBLL vehicleBLL = new vehicleBLL();
        private ParkingCardBLL parkingCardBLL = new ParkingCardBLL();
        private parkingSlotBLL parkingSlotBLL = new parkingSlotBLL();
        public frmInOutVehicle()
        {
            InitializeComponent();
            LoadAllVehicleType();
            timerClock.Interval = 1000;
            timerClock.Tick += TimerClock_Tick;
            timerClock.Start();
            kryptonGroupBox4.BackgroundImageLayout = ImageLayout.Stretch;
            kDTPInfoIn.Format = DateTimePickerFormat.Custom;
            kDTPInfoIn.CustomFormat = "yyyy-MM-dd HH:mm:ss";

            kDTPInfoOut.Format = DateTimePickerFormat.Custom;
            kDTPInfoOut.CustomFormat = "yyyy-MM-dd HH:mm:ss";
        }
        private Guid? selectedAreaId;

        private void frmInOutVehicle_Load(object sender, EventArgs e)
        {
            LoadUserSession();
        }
        private void LoadAreas()
        {
            var parkingAreaRepo = new ParkingAreaRepository();

            // Lấy tất cả các bãi đỗ xe (không lọc theo status)
            var parkingAreas = parkingAreaRepo.GetAllParkingAreas(); // đây là hàm của bạn

            comboBoxAreas.DataSource = parkingAreas;
            comboBoxAreas.DisplayMember = "AreaName";
            comboBoxAreas.ValueMember = "Id";
        }
        private void LoadUserSession()
        {
            LoadAreas(); // Load combobox trước

            if (SessionManager.CurrentUser == null) return;

            selectedAreaId = SessionManager.CurrentUser.AreaId ?? Guid.Empty;

            // Set combobox về đúng khu vực của user
            comboBoxAreas.SelectedValue = selectedAreaId.ToString();

            LoadAvailableSlots(selectedAreaId.Value);
            LoadParkingSlots(selectedAreaId.Value);
        }

        private void RefreshForm()
        {
            kTbInBks.Text = "";
            kTbOutBks.Text = "";
            LoadAvailableSlots(selectedAreaId ?? Guid.Empty);
        }

        private void LoadAllVehicleType()
        {
            try
            {
                List<VehicleType> vehicleTypes = vehicleBLL.GetAllVehicleType();

                kCbbTypeVehicle.DataSource = vehicleTypes;
                kCbbTypeVehicle.DisplayMember = "VehicleTypeName";
                kCbbTypeVehicle.ValueMember = "Id";
                kCbbTypeVehicle.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách loại xe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadParkingCardInfo(string licensePlate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licensePlate))
                {
                    MessageBox.Show("Biển số xe không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //MessageBox.Show($"Đang tìm thẻ xe cho biển số: {licensePlate}", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);

                parkingCardDetail cardDetail = parkingCardBLL.GetParkingCardByLicensePlate(licensePlate);

                if (cardDetail == null)
                {
                    MessageBox.Show("Không tìm thấy thẻ xe cho biển số này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoadInfoCard();
                    return;
                }

                string license = cardDetail.LicensePlate ?? "N/A";
                string userCode = cardDetail.UserCode ?? "N/A";
                decimal price = cardDetail.Price;
                DateTime startDate = cardDetail.StartDate;
                DateTime endDate = cardDetail.EndDate ?? DateTime.Now;
                bool isMonth = cardDetail.IsMonth;

                //MessageBox.Show($@"
                //    Dữ liệu lấy được:
                //    Biển số: {license}
                //    User Code: {userCode}
                //    Giá: {price:N0} VNĐ
                //    Ngày bắt đầu: {startDate}
                //    Ngày kết thúc: {endDate}
                //", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);

                kTbInfoBs.Text = license;
                kTbInfoUser.Text = userCode;
                kTbInfoPrice.Text = price.ToString("N0") + " VNĐ";
                kDTPInfoIn.Value = startDate;
                kDTPInfoOut.Value = endDate;
                kRbtnInfoMonth.Checked = isMonth;
                kRBtnInfoDay.Checked = !isMonth;

                //MessageBox.Show("Dữ liệu đã được hiển thị trên giao diện.", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi xảy ra: {ex.Message}\n{ex.StackTrace}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAvailableSlots(Guid areaId)
        {
            try
            {
                List<ParkingSlotModel> slots = parkingSlotBLL.GetSlotByArea(areaId);

                kCbbSlotNum.DataSource = null;
                kCbbSlotNum.Items.Clear();

                if (slots.Count == 0)
                {
                    MessageBox.Show("Không có vị trí đỗ nào sẵn sàng trong khu vực này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                kCbbSlotNum.DataSource = slots;
                kCbbSlotNum.DisplayMember = "SlotNumber";
                kCbbSlotNum.ValueMember = "Id";

                kCbbSlotNum.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách vị trí: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TimerClock_Tick(object sender, EventArgs e)
        {
            lblClock.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm:ss");
        }

        private void kTbInBks_TextChanged(object sender, EventArgs e)
        {
            kLbIbBs.Text = kTbInBks.Text;
        }

        private void kryptonLabel7_Click(object sender, EventArgs e)
        {

        }

        private void LoadInfoCard()
        {
            kTbInfoBs.Text = "";
            kTbInfoUser.Text = "";
            kTbInfoPrice.Text = "";
            kDTPInfoIn.Value = DateTime.Now;
            kDTPInfoOut.Value = DateTime.Now;
        }
        private void kBtnSubmitBs_Click(object sender, EventArgs e)
        {
            string licensePlate = kTbInBks.Text.Trim();
            string status = "parked";

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                MessageBox.Show("Biển số xe không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Guid? vehicleId = vehicleBLL.GetVehicleIdByLicensePlate(licensePlate);
                string currentStatus = vehicleBLL.GetVehicleStatusByLicensePlate(licensePlate);
                Guid selectedAreaId = SessionManager.CurrentUser.AreaId ?? Guid.Empty;

                if (vehicleId != null && currentStatus == "parked")
                {
                    MessageBox.Show("Xe đã trong bãi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (selectedAreaId == Guid.Empty)
                {
                    MessageBox.Show("Không thể xác định khu vực đỗ xe!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Guid.TryParse(kCbbSlotNum.SelectedValue?.ToString(), out Guid parkingSlotId))
                {
                    MessageBox.Show("ID chỗ đỗ không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!Guid.TryParse(kCbbTypeVehicle.SelectedValue?.ToString(), out Guid vehicleTypeId))
                {
                    MessageBox.Show("ID loại xe không hợp lệ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DateTime now = DateTime.Now;

                if (vehicleId == null || currentStatus == "exited")
                {
                    VehicleModel vehicle = new VehicleModel(
                        Guid.NewGuid(),
                        licensePlate,
                        vehicleTypeId,
                        parkingSlotId,
                        selectedAreaId,
                        now,
                        null,
                        now,
                        now,
                        now
                    );

                    vehicleBLL.CreateVehicle(vehicle);
                    vehicleId = vehicle.Id;
                }

                //vehicleBLL.UpdateVehicleStatusByLicensePlate(licensePlate, status);
                parkingCardBLL.CreateParkingCard(licensePlate, "", false);
                LoadParkingSlots(selectedAreaId);
                LoadParkingCardInfo(licensePlate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm phương tiện hoặc thẻ gửi xe: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            RefreshForm();
        }

        private void kBtnOutBks_Click(object sender, EventArgs e)
        {
            string licensePlate = kTbOutBks.Text.Trim();

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                MessageBox.Show("Biển số xe không được để trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra xe có tồn tại hay không
            var parkingCard = parkingCardBLL.GetParkingCardByLicensePlate(licensePlate);
            if (parkingCard == null)
            {
                MessageBox.Show("Không tìm thấy xe có biển số này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (parkingSlotBLL == null)
            {
                MessageBox.Show("Lỗi: parkingSlotBLL chưa được khởi tạo!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Cập nhật trạng thái chỗ đỗ
            parkingSlotBLL.updateSlotStatusByVehicle(licensePlate);

            // Load lại dữ liệu và form
            LoadParkingCardInfo(licensePlate);
            LoadParkingSlots(selectedAreaId.Value);
            RefreshForm();

            // Mở form thanh toán
            frmCreatePayment createPaymentForm = new frmCreatePayment(licensePlate);
            createPaymentForm.StartPosition = FormStartPosition.CenterParent;
            createPaymentForm.ShowDialog();
        }

        private void kTbOutBks_TextChanged(object sender, EventArgs e)
        {
            kLbOutTextBs.Text = kTbOutBks.Text;
        }

        private void LoadParkingSlots(Guid areaId)
        {
            List<ParkingSlotModel> slots = parkingSlotBLL.GetSlotArea(areaId);
            List<ParkingSlotModel> allSlots = new List<ParkingSlotModel>();
            int totalSlots = 20;
            int availableSlots = 0;
            for (int i = 1; i <= 20; i++)
            {
                var existingSlot = slots.FirstOrDefault(s => s.SlotNumber == i);
                if (existingSlot != null)
                {
                    allSlots.Add(existingSlot);
                }
                else
                {
                    allSlots.Add(new ParkingSlotModel(
                        Guid.NewGuid(),
                        i,
                        "Unknown",
                        "Sửa chữa",
                        areaId
                    ));
                }
            }

            availableSlots = allSlots.Count(s => s.SlotStatus.Trim().ToLower() == "sẵn sàng");

            kLbCountSlot.Text = $"Chỗ trống: {availableSlots} / {totalSlots}";

            kTLPslotArea.Controls.Clear();
            kTLPslotArea.ColumnStyles.Clear();
            kTLPslotArea.RowStyles.Clear();

            kTLPslotArea.ColumnCount = 5;
            kTLPslotArea.RowCount = 4;
            kTLPslotArea.Dock = DockStyle.Fill;

            for (int i = 0; i < kTLPslotArea.ColumnCount; i++)
            {
                kTLPslotArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / kTLPslotArea.ColumnCount));
            }
            for (int i = 0; i < kTLPslotArea.RowCount; i++)
            {
                kTLPslotArea.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / kTLPslotArea.RowCount));
            }

            int count = 0;
            foreach (var slot in allSlots)
            {
                Button btn = new Button();
                btn.Text = $"Slot {slot.SlotNumber}";
                btn.Tag = slot.Id;
                btn.Dock = DockStyle.Fill;
                btn.Margin = new Padding(5);
                btn.Font = new Font("Arial", 10, FontStyle.Bold);
                btn.Click += Btn_Click;

                string status = slot.SlotStatus.Trim().ToLower();
                switch (status)
                {
                    case "đã có xe":
                        btn.BackColor = Color.Red;
                        break;
                    case "sẵn sàng":
                        btn.BackColor = Color.Green;
                        break;
                    case "sửa chữa":
                    default:
                        btn.BackColor = Color.Yellow;
                        break;
                }

                kTLPslotArea.Controls.Add(btn, count % 5, count / 5);
                count++;
            }
            LoadSlotDescription();
        }


        private void Btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                Guid slotId = (Guid)btn.Tag;
                MessageBox.Show($"Slot ID: {slotId}", "Thông tin", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void LoadSlotDescription()
        {
            kTLPdescription.Controls.Clear();
            kTLPdescription.ColumnCount = 1;
            kTLPdescription.RowCount = 3;
            kTLPdescription.Dock = DockStyle.Fill;

            var statuses = new (string Text, Color Color)[]
            {
                ("Đã có xe", Color.Red),
                ("Sẵn sàng", Color.Green),
                ("Sửa chữa", Color.Yellow)
            };

            kTLPdescription.ColumnStyles.Clear();
            kTLPdescription.RowStyles.Clear();

            for (int i = 0; i < statuses.Length; i++)
            {
                kTLPdescription.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / statuses.Length));
            }

            for (int i = 0; i < statuses.Length; i++)
            {
                Button btn = new Button();
                btn.Text = statuses[i].Text;
                btn.BackColor = statuses[i].Color;
                btn.Dock = DockStyle.Fill;
                btn.Margin = new Padding(5);
                btn.Enabled = false;

                kTLPdescription.Controls.Add(btn, 0, i);
            }
        }

        private void kBtnRefreshmap_Click(object sender, EventArgs e)
        {
            LoadParkingSlots(selectedAreaId.Value);
        }

        private void kTbBillPay_Click(object sender, EventArgs e)
        {
            string licensePlate = kTbInfoBs.Text.Trim();

            frmCreatePayment createPaymentForm = new frmCreatePayment(licensePlate);
            createPaymentForm.StartPosition = FormStartPosition.CenterParent;
            createPaymentForm.ShowDialog();
        }

        private void kryptonGroupBox5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void kryptonGroupBox3_Panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboBoxAreas_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBoxAreas.SelectedValue is string selectedId)
            {
                selectedAreaId = Guid.Parse(selectedId); // nếu id dạng Guid string
                LoadAvailableSlots(selectedAreaId.Value);
                LoadParkingSlots(selectedAreaId.Value);
            }
        }
    }
}
