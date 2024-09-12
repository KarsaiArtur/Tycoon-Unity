// See https://aka.ms/new-console-template for more information

using System.Data;
using System.Runtime.InteropServices;

public class MyProgram{

    class Attribute{
        public string type;
        public string name;

        public Attribute(string type, string name){
            this.type = type;
            this.name = name;
        }
    }

    static List<Attribute> attributes;

    static void Main(){
        string basepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        basepath = basepath.Substring(0, basepath.IndexOf("CodeGenerator"))+"Assets\\Scripts\\";
        //basepath = basepath.Substring(0, basepath.IndexOf("CodeGenerator"))+"Assets\\Scripts\\" + "ZooManager.cs";

        List<string> files = Directory.GetFiles(basepath).ToList();
        files = files.Where(file => file.EndsWith(".cs")).ToList();

        foreach(var file in files){
            generateCode(file);

        }
    }

    static void generateCode(string path){
        attributes = new List<Attribute>();

        string fileContent = "";
        if(File.Exists(path)){
            fileContent = File.ReadAllText(path);
        }
        else{
            return;
        }

        string temp = "";

        int classIndex = fileContent.IndexOf("class");
        if(classIndex >= 0){
            temp = fileContent.Substring(fileContent.IndexOf("class"));
            temp = temp[..temp.IndexOf("{")];
        }

        if(!temp.Contains("Saveable")){
            System.Console.WriteLine("RETURNED");
            return;
        }

        System.Console.WriteLine("RUN");
        System.Console.WriteLine(path);
        string end = fileContent.Substring(fileContent.LastIndexOf("}"));

        string attributesString = fileContent.Substring(fileContent.IndexOf("//////"));
        attributesString = attributesString.Substring(0, attributesString.LastIndexOf("/////")).Replace("/", "");
        
        foreach(var st in attributesString.Split(",")){
            string[] attribute = st.Split(" ");
            attributes.Add(new Attribute(attribute[0], attribute[1]));
        }
        

        
        string generatedCode = 
@"///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

";  
    generatedCode += writeDataClass();
    generatedCode += writeDataToJson();
    generatedCode += writeFromJson();
    string fileName = path.Substring(path.LastIndexOf("\\") + 1);
    fileName = fileName.Substring(0, fileName.IndexOf(".cs")) +".json";
    generatedCode += writeGetFileName(fileName);
    generatedCode += writeSetData();

        string editedCode = fileContent.Substring(0, fileContent.LastIndexOf("}"));

        
        if(editedCode.Contains("///******************************")){
                editedCode = editedCode.Substring(0, editedCode.IndexOf("///******************************"));
        }

        string newContent = editedCode + generatedCode + end;

        File.WriteAllText(path, newContent);
    }

    static string attributesWithType(){
        string _string = $"{attributes[0].type} {attributes[0].name}";
        foreach(var attribute in attributes.Skip(1)){
            _string += $", {attribute.type} {attribute.name}";
        }
        return _string;
    }

     static string attributesWithoutType(string beforeAttribute = "", bool isSet = false){
        string _string ="";
        if(isSet){
            string isNew = "";
            string isNewAfter = "";
            switch(attributes[0].type){
                case "DateTime":
                    isNew = "new DateTime(";
                    isNewAfter = ")";
                    break;
                default:
                    isNew = "";
                    isNewAfter = "";
                    break;
            }

            _string = $"{isNew}{beforeAttribute}{attributes[0].name}{isNewAfter}";
            foreach(var attribute in attributes.Skip(1)){
                isNew = attributes[0].type.Equals("DateTime") ? "new DateTime(" : "";
                isNewAfter = attributes[0].type.Equals("DateTime") ? ")" : "";
                _string += $", {isNew}{beforeAttribute}{attribute.name}{isNewAfter}";
            }
            return _string;
        }

        _string = $"{beforeAttribute}{attributes[0].name}";
        foreach(var attribute in attributes.Skip(1)){
            _string += $", {beforeAttribute}{attribute.name}";
        }
        return _string;
    }

    static string attributesInLines(bool withAfterAttribute = false){
        string _string = "";
        foreach(var attribute in attributes){
            string finalAttribute = "";
            if(withAfterAttribute){
                finalAttribute = attribute.type switch
                {
                    "DateTime" => ".Ticks",
                    _ => "",
                };
            }
            
            string attributeString =
$"           this.{attribute.name} = {attribute.name}{finalAttribute};";
            _string += System.Environment.NewLine + attributeString;
        }
        return _string;
    }

    static string writeDataClass(){
        string classString = 
@"    class Data
    {";
        foreach(var attribute in attributes){
            string finalAttribute = "";
            finalAttribute = attribute.type switch
            {
                "DateTime" => "long",
                _ => attribute.type,
            };
            string attributeString =
$"        public {finalAttribute} {attribute.name};";
            classString += System.Environment.NewLine + attributeString;
        }
        classString += System.Environment.NewLine  + System.Environment.NewLine +
@"        public Data(";
        classString += attributesWithType();
        classString+=@")
        {";
        classString += attributesInLines(true);
        classString += @"
        }
    }

    Data data;
";
        return classString;
    }
    
    static string writeDataToJson(){
        return
@"    
    public string DataToJson(){
        Data data = new Data("+ attributesWithoutType() +@");
        return JsonUtility.ToJson(data);
    }
";
    }

    static string writeFromJson(){
        return 
@"    
    public void FromJson(string json){
        data = JsonUtility.FromJson<Data>(json);
        SetData("+ attributesWithoutType("data.", true) +@");
    }
";
    }

    static string writeGetFileName(string fileName){
        return 
@"    
    public string GetFileName(){
        return " + $"\"{fileName}\"" + @";
    }
";
    }

    static string writeSetData(){
        return 
@"    
    void SetData("+ attributesWithType() +@"){ 
        "+attributesInLines()+@"
    }
";
    }
}

