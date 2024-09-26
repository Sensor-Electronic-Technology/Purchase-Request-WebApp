﻿namespace FileStorage.SignatureVerify.FileTypes;

public sealed class Pdf : FileType {
    public Pdf() {
        Name = "PDF";
        Description = "PDF FILE";
        AddExtensions("pdf");
        AddSignatures(
            "%PDF"u8.ToArray()
        );
    }
}

public sealed class Png : FileType {
    public Png() {
        Name = "PNG";
        Description = "PNG Image";
        AddExtensions("png");
        AddSignatures(
            "%PNG"u8.ToArray()
        );
    }
}

public sealed class Xls : FileType {
    public Xls() {
        Name = "XLS";
        Description = "XLS MICROSOFT OFFICE DOCUMENT";
        AddExtensions("xls");
        AddSignatures(
            new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },
            new byte[] { 0x09, 0x08, 0x10, 0x00, 0x00, 0x06, 0x05, 0x00 },
            new byte[] { 0xFD, 0xFF, 0xFF, 0xFF, 0x10 },
            new byte[] { 0xFD, 0xFF, 0xFF, 0xFF, 0x1F },
            new byte[] { 0xFD, 0xFF, 0xFF, 0xFF, 0x22 },
            new byte[] { 0xFD, 0xFF, 0xFF, 0xFF, 0x23 },
            new byte[] { 0xFD, 0xFF, 0xFF, 0xFF, 0x28 },
            new byte[] { 0xFD, 0xFF, 0xFF, 0xFF, 0x29 }
        );
    }
}

public sealed class Xlsx : FileType {
    public Xlsx() {
        Name = "XLSX";
        Description = "XLSX MICROSOFT OFFICE DOCUMENT";
        AddExtensions("xlsx");
        AddSignatures(
            new byte[] { 0x50, 0x4B, 0x03, 0x04 },
            new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 }
        );
    }
}

public sealed class Xml : FileType {
    public Xml() {
        Name = "XML";
        Description = "XML IMAGE";
        AddExtensions("xml");
        AddSignatures(
            "<?xml version=\"1.0\"?>"u8.ToArray()
        );
    }
}

public sealed class Jpeg : FileType {
    public Jpeg() {
        Name = "JPEG";
        Description = "JPEG IMAGE";
        AddExtensions("jpeg", "jpg");
        AddSignatures(
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }
        );
    }
}

public sealed class Jpg : FileType {
    public Jpg() {
        Name = "JPG";
        Description = "JPG IMAGE";
        AddExtensions("jpg");
        AddSignatures(
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
            new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
        );
    }
}

public sealed class Doc : FileType {
    public Doc() {
        Name = "DOC";
        Description = "DOC MICROSOFT OFFICE DOCUMENT";
        AddExtensions("doc");
        AddSignatures(
            new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 },
            "\rDOC"u8.ToArray(),
            new byte[] { 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00 },
            new byte[] { 0xDB, 0xA5, 0x2D, 0x00 },
            new byte[] { 0xEC, 0xA5, 0xC1, 0x00 }
        );
    }
}

public sealed class Msg : FileType {
    public Msg() {
        Name = "MSG";
        Description = "MSG MICROSOFT OFFICE DOCUMENT";
        AddExtensions("msg");
        AddSignatures(
            "\rMSG"u8.ToArray()
        );
    }
}

public sealed class Docx : FileType {
    public Docx() {
        Name = "DOCX";
        Description = "DOCX MICROSOFT OFFICE DOCUMENT";
        AddExtensions("docx");
        AddSignatures(
            new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00 },
            new byte[] { 0x50, 0x4B, 0x03, 0x04 }
        );
    }
}

public sealed class Bmp : FileType {
    public Bmp() {
        Name = "BMP";
        Description = "BMP IMAGE";
        AddExtensions("bmp");
        AddSignatures(
            "BM"u8.ToArray()
        );
    }
}

public sealed class Gif : FileType
{
    public Gif()
    {
        Name = "GIF";
        Description = "GIF IMAGE";
        AddExtensions("gif");
        AddSignatures(
            "GIF8"u8.ToArray()
        );
    }
}