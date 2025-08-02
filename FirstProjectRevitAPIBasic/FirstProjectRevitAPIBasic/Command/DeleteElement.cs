using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Linq;

namespace FirstProjectRevitAPIBasic.Command
{
    [Transaction(TransactionMode.Manual)]
    internal class DeleteElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Người dùng chọn một phần tử
                Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, "Chọn phần tử để xóa (Tường, Cột, Sàn, Cửa, Cửa sổ)");
                if (pickedRef == null) return Result.Cancelled;

                Element element = doc.GetElement(pickedRef);

                // 2. Cho phép xóa các loại sau:
                BuiltInCategory[] allowedCategories = new BuiltInCategory[]
                {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_StructuralColumns,
                    BuiltInCategory.OST_Floors,
                    BuiltInCategory.OST_Doors,
                    BuiltInCategory.OST_Windows
                };

                bool isAllowed = element.Category != null &&
                                 allowedCategories.Contains((BuiltInCategory)element.Category.Id.IntegerValue);

                if (!isAllowed)
                {
                    TaskDialog.Show("Thông báo", "Chỉ hỗ trợ xóa các phần tử: Tường, Cột, Sàn, Cửa, Cửa sổ.");
                    return Result.Cancelled;
                }

                // 3. Thực hiện xóa
                using (Transaction trans = new Transaction(doc, "Xóa phần tử"))
                {
                    trans.Start();

                    doc.Delete(element.Id);

                    trans.Commit();
                }

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
