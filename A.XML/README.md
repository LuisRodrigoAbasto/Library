#  A.XML

## Why choose A.XML?

### Free

Abasto Excel are open source and free for commercial use. You can install them from [NuGet](https://www.nuget.org/packages/A.XML)

### Examples
```cs
var excel=ExcelXml.ReadSpreadSheet<Employe>(x=>{
        x.Path="..\\..\\Employes.xlsx";
        x.ColumnsAll=true;
    });
Or
var excel=ExcelXml.ReadSpreadSheet(x=>{
        x.Stream=File.OpenRead("..\\..\\Employes.xlsx");
        x.ColumnsAll=true;
    });

```