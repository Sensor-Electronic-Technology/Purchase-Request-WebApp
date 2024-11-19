using Ardalis.SmartEnum;
namespace Webapp.Data;

public class FileDisplayType:SmartEnum<FileDisplayType,string> {
    public static readonly FileDisplayType PDF=new(nameof(PDF),"PDF",[".pdf"]);
    public static readonly FileDisplayType IMAGE=new(nameof(IMAGE),"Image",[".jpg",".jpeg",".png"]);
    public static readonly FileDisplayType DOC=new(nameof(DOC),"Document",[".doc",".docx"]);
    public static readonly FileDisplayType OTHER=new(nameof(OTHER),"Other",[".xls",".xlsx"]);
    public IEnumerable<string> Extensions;

    private FileDisplayType(string name, string value, params IEnumerable<string> ext) : base(name, value) {
        this.Extensions = ext;
    }
}