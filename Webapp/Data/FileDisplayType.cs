using Ardalis.SmartEnum;
namespace Webapp.Data;

public class FileDisplayType:SmartEnum<FileDisplayType,string> {
    public static readonly FileDisplayType PDF=new(nameof(PDF),"PDF",[".pdf"]);
    public static readonly FileDisplayType IMAGE=new(nameof(IMAGE),"Image",[".jpg",".jpeg",".png"]);
    public static readonly FileDisplayType DOC=new(nameof(DOC),"Document",[".doc",".docx"]);
    public static readonly FileDisplayType OTHER=new(nameof(OTHER),"Other",[".xls",".xlsx"]);
    public IEnumerable<string> Extensions;
    public string Icon=>Value switch {
        "PDF"=>"picture_as_pdf",
        "Image"=>"image",
        "Document"=>"article",
        "Other"=>"table",
        _=>"fa-file"
    };
    
    public static FileDisplayType FromExtension(string ext) {
        var fileDisplayType= List.FirstOrDefault(x=>x.Extensions.Contains(ext));
        return fileDisplayType ?? OTHER;
    }

    private FileDisplayType(string name, string value, params IEnumerable<string> ext) : base(name, value) {
        this.Extensions = ext;
    }
}