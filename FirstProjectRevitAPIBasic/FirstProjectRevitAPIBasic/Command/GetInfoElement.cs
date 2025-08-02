using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI.Selection;

namespace FirstProjectRevitAPIBasic.Command
{
    [Transaction(TransactionMode.Manual)]
    internal class GetInfoElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uidoc = uiApp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // 1. Chọn phần tử bất kỳ
                Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, "Chọn phần tử để xem thông tin");
                if (pickedRef == null) return Result.Cancelled;

                Element element = doc.GetElement(pickedRef);

                // 2. Lấy thông tin cơ bản
                string categoryName = element.Category?.Name ?? "Không xác định";
                string elementName = element.Name;
                string elementId = element.Id.IntegerValue.ToString();

                string familyName = "";
                string typeName = "";

                // Nếu là family instance thì lấy thêm thông tin loại và family
                if (element is FamilyInstance famInst)
                {
                    familyName = famInst.Symbol.FamilyName;
                    typeName = famInst.Symbol.Name;
                }
                else if (element is ElementType typeElem)
                {
                    familyName = typeElem.FamilyName;
                    typeName = typeElem.Name;
                }

                string info = $"Category: {categoryName}\n"
                            + $"Family: {familyName}\n"
                            + $"Type: {typeName}\n"
                            + $"Instance Name: {elementName}\n"
                            + $"Element ID: {elementId}";

                // 3. Hiển thị
                TaskDialog.Show("Thông tin phần tử", info);

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
