using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;

namespace FirstProjectRevitAPIBasic.Command
{
    [Transaction(TransactionMode.Manual)]
    internal class EditElementCoordinates : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Cho người dùng chọn một phần tử trong mô hình
                Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, "Chọn một tường, cột hoặc sàn để di chuyển");
                if (pickedRef == null) return Result.Cancelled;

                Element element = doc.GetElement(pickedRef);

                // 2. Kiểm tra nếu là Wall, Column, hoặc Floor
                if (!(element is Wall || element is Floor || (element is FamilyInstance fi && fi.StructuralType == StructuralType.Column)))
                {
                    TaskDialog.Show("Thông báo", "Chỉ hỗ trợ di chuyển Tường, Cột hoặc Sàn.");
                    return Result.Cancelled;
                }

                // 3. Dịch chuyển theo vector (5m theo X => 5.0)
                XYZ moveVector = new XYZ(5.0, 0, 0); // đơn vị là feet, nếu bạn cần 5 mét: 5m = ~16.4 feet

                using (Transaction trans = new Transaction(doc, "Di chuyển phần tử"))
                {
                    trans.Start();

                    ElementTransformUtils.MoveElement(doc, element.Id, moveVector);

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
