using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FirstProjectRevitAPIBasic.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FirstProjectRevitAPIBasic.UI
{
    public partial class FormMutil_ElementManager : System.Windows.Forms.Form
    {
        private Document _doc;
        private Dictionary<string, BuiltInCategory> _categoryMap;
        public List<Level> _levels;

        public FormMutil_ElementManager(Document doc, List<BuiltInCategory> categories, List<Level> levels)
        {
            InitializeComponent();
            _doc = doc;
            _levels = levels;

            // Khởi tạo từ List sang Dictionary hiển thị thân thiện
            _categoryMap = categories.ToDictionary(
                cat => LabelUtils.GetLabelFor(cat),
                cat => cat
            );

            // Hiển thị tên thân thiện trên ComboBox
            cbbCategory.DataSource = new BindingSource(_categoryMap, null);
            cbbCategory.DisplayMember = "Key";
            cbbCategory.ValueMember = "Value";
            // Hiển thị tên thân thiện trên ComboBox cho Level
            cbbLevel.DataSource = _levels;
            cbbLevel.DisplayMember = "Name";

            btnSetMark.Enabled = false;
        }

        public BuiltInCategory GetSelectedCategory()
        {
            return (BuiltInCategory)((KeyValuePair<string, BuiltInCategory>)cbbCategory.SelectedItem).Value;
        }

        private void btnFilterElement_Click(object sender, EventArgs e)
        {
            dgvElement.Rows.Clear(); // Xóa các dòng cũ trước khi load mới

            BuiltInCategory selectedCategory = GetSelectedCategory();
            Level selectedLevel = cbbLevel.SelectedItem as Level;

            List<Element> elements = Common.GetElementsByCategoryAndLevel(_doc, selectedCategory, selectedLevel);

            foreach (Element el in elements)
            {
                int rowIndex = dgvElement.Rows.Add();
                dgvElement.Rows[rowIndex].Cells["ColSelect"].Value = false; // Checkbox mặc định chưa chọn
                dgvElement.Rows[rowIndex].Cells["ColElements"].Value = el.Name; // Hiển thị tên Element
                dgvElement.Rows[rowIndex].Tag = el.Id; // Gán ElementId vào dòng (sử dụng sau)
            }
        }

        private void btnDeleteElement_Click(object sender, EventArgs e)
        {
            List<ElementId> toDeleteIds = new List<ElementId>();
            List<string> deletedElementNames = new List<string>();

            foreach (DataGridViewRow row in dgvElement.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells["ColSelect"].Value);
                if (isChecked)
                {
                    ElementId id = row.Tag as ElementId;
                    if (id != null)
                    {
                        toDeleteIds.Add(id);

                        // Lấy tên từ cell thay vì Element
                        string name = row.Cells["ColElements"].Value?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            deletedElementNames.Add(name);
                        }
                    }
                }
            }

            if (toDeleteIds.Any())
            {
                Common.DeleteElements(_doc, toDeleteIds);

                string message = $"Đã xóa {toDeleteIds.Count} element:\n- " + string.Join("\n- ", deletedElementNames);
                MessageBox.Show(message);

                btnFilterElement_Click(null, null); // Refresh lại bảng
            }
            else
            {
                MessageBox.Show("Chưa chọn element nào.");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Đóng form mà không thực hiện hành động gì
            Close();
        }

        private void btnEditElement_Click(object sender, EventArgs e)
        {
            string markText = tbSetMark.Text.Trim();

            if (string.IsNullOrEmpty(markText))
            {
                MessageBox.Show("Vui lòng nhập nội dung Mark.");
                return;
            }

            List<ElementId> selectedIds = new List<ElementId>();

            foreach (DataGridViewRow row in dgvElement.Rows)
            {
                bool isChecked = Convert.ToBoolean(row.Cells["ColSelect"].Value);
                if (isChecked && row.Tag is ElementId id)
                {
                    selectedIds.Add(id);
                }
            }

            if (selectedIds.Any())
            {
                Common.SetMarkForElements(_doc, selectedIds, markText);
                MessageBox.Show($"Đã gán Mark \"{markText}\" cho {selectedIds.Count} element.");
            }
            else
            {
                MessageBox.Show("Chưa chọn element nào.");
            }
        }

        private void tbSetMark_TextChanged(object sender, EventArgs e)
        {
            btnSetMark.Enabled = !string.IsNullOrWhiteSpace(tbSetMark.Text);
        }
    }
}