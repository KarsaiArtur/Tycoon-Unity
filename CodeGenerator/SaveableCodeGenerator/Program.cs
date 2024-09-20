// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;



public class MyProgram{
    static List<string> primitives = ["bool", "string", "int", "float", "long", "Vector3", "Vector3[]", "Quaternion", "DateTime"];

    class Attribute{
        public string type;
        public string name;
        public bool isList;
        public bool isPrimitive;

        public Attribute(string type, string name, bool isList, bool isPrimitive){
            this.type = type;
            this.name = name;
            this.isList = isList;
            this.isPrimitive = isPrimitive;
        }
    }

    static List<Attribute> attributes;
    static bool isSerializable;

    static void Main(){
        string basepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
        basepath = basepath.Substring(0, basepath.IndexOf("CodeGenerator"))+"Assets\\Scripts\\";
        //basepath = basepath.Substring(0, basepath.IndexOf("CodeGenerator"))+"Assets\\Scripts\\" + "ZooManager.cs";

        List<string> files = Directory.GetFiles(basepath).ToList();
        files = files.Where(file => file.EndsWith(".cs")).ToList();

        foreach(var file in files){
            isSerializable = false;
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
            return;
        }

        string className = temp.Substring("class".Length);
        className = className.Substring(0, className.IndexOf(":")).Replace(" ", "");

        string end = fileContent.Substring(fileContent.LastIndexOf("}"));

        string attributesString = fileContent.Substring(fileContent.IndexOf("//////"));
        attributesString = attributesString.Substring(0, attributesString.LastIndexOf("/////")).Replace("/", "").Replace(System.Environment.NewLine, "");
        
        foreach(var st in attributesString.Split(",")){
            string[] attribute = st.Split(" ");
            bool isList = false;
            if(attribute[0].Contains("List")){
                attribute[0] = attribute[0].Substring(attribute[0].IndexOf("<")+1);
                attribute[0] = attribute[0].Substring(0, attribute[0].IndexOf(">"));
                isList = true;
            }
            
            bool isPrimitive = primitives.Contains(attribute[0]) ? true : false;
            attributes.Add(new Attribute(attribute[0], attribute[1], isList, isPrimitive));
        }

        
        if(fileContent.Contains("SERIALIZABLE")){
            string serializable = fileContent.Substring(fileContent.IndexOf("SERIALIZABLE:"));
            serializable = serializable.Substring(0, serializable.IndexOf("/"));
            isSerializable = serializable.Contains("YES") ? true : false;
        }
        
        string generatedCode = 
@"///******************************
    ///GENERATED CODE, DONT MODIFY
    ///******************************

";  
    generatedCode += writeDataClass(className);
    generatedCode += writeDataToJson(className);
    generatedCode += writeFromJson(className);
    string fileName = path.Substring(path.LastIndexOf("\\") + 1);
    fileName = fileName.Substring(0, fileName.IndexOf(".cs")) +".json";
    generatedCode += writeGetFileName(fileName);
    generatedCode += writeSetData();
    if(isSerializable){
        generatedCode += writeToDataClass(className);
        generatedCode += writeFromDataClass(className);
    }

        string editedCode = fileContent.Substring(0, fileContent.LastIndexOf("}"));

        
        if(editedCode.Contains("///******************************")){
                editedCode = editedCode.Substring(0, editedCode.IndexOf("///******************************"));
        }

        string newContent = editedCode + generatedCode + end;

        File.WriteAllText(path, newContent);
    }

    static string attributesWithType(){
        var finalAttribute = attributes[0].isPrimitive ? attributes[0].type : attributes[0].type + "Data";
        finalAttribute = attributes[0].isList ? $"List<{finalAttribute}>" : finalAttribute;
        string _string = $"{finalAttribute} {attributes[0].name}";
        foreach(var attribute in attributes.Skip(1)){
            finalAttribute = attribute.isPrimitive ? attribute.type : attribute.type + "Data";
            finalAttribute = attribute.isList ? $"List<{finalAttribute}>" : finalAttribute;
            _string += $", {finalAttribute} {attribute.name}";
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
                isNew = attribute.type.Equals("DateTime") ? "new DateTime(" : "";
                isNewAfter = attribute.type.Equals("DateTime") ? ")" : "";
                _string += $", {isNew}{beforeAttribute}{attribute.name}{isNewAfter}";
            }
            return _string;
        }

        string attributeName = (attributes[0].name.Equals("position") || attributes[0].name.Equals("rotation")) && !isSet ? $"transform.{attributes[0].name}" : attributes[0].name;
        _string = $"{beforeAttribute}{attributeName}";
        foreach(var attribute in attributes.Skip(1)){
            attributeName = (attribute.name.Equals("position") || attribute.name.Equals("rotation")) && !isSet ? $"transform.{attribute.name}"  : attribute.name;
            _string += $", {beforeAttribute}{attributeName}";
        }
        return _string;
    }

    static string attributesInLines(bool withAfterAttribute = false){
        string _string = "";
        foreach(var attribute in attributes){
            string attriButeName = (attribute.name.Equals("position") || attribute.name.Equals("rotation")) && !withAfterAttribute ? $"transform.{attribute.name}" : attribute.name;
            string finalAttribute = "";
            if(withAfterAttribute){
                finalAttribute = attribute.type switch
                {
                    "DateTime" => ".Ticks",
                    _ => "",
                };
            }
            
            string attributeString =
$"           this.{attriButeName} = {attribute.name}{finalAttribute};";
            _string += System.Environment.NewLine + attributeString;
        }
        return _string;
    }

    static string writeDataClass(string className){
        string classString = 
(isSerializable ? @"    [Serializable]
" : "")
+ $"    public class {className}Data"+@"
    {";
        foreach(var attribute in attributes){
            string finalAttribute = "";
            finalAttribute = attribute.type switch
            {
                "DateTime" => "long",
                _ => attribute.type,
            };
            finalAttribute = attribute.isPrimitive ? finalAttribute : finalAttribute + "Data";
            finalAttribute = attribute.isList ? $"List<{finalAttribute}>" : finalAttribute;
            string attributeString =
$"        public {finalAttribute} {attribute.name};";
            classString += System.Environment.NewLine + attributeString;
        }
        classString += System.Environment.NewLine  + System.Environment.NewLine +
$"        public {className}Data(";
        classString += attributesWithType();
        classString+=@")
        {";
        classString += attributesInLines(true);
        classString += @"
        }
    }

"+$"    {className}Data data; "+@"
";
        return classString;
    }
    
    static string writeDataToJson(string className){
        return
@"    
    public string DataToJson(){
"+toDataList()+$"        {className}Data data = new {className}Data("+ attributesWithoutType() +@");
        return JsonUtility.ToJson(data);
    }
";
    }

    static string writeFromJson(string className){
        return 
@"    
    public void FromJson(string json){
"+$"        data = JsonUtility.FromJson<{className}Data>(json);"+@"
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
        "+fromDataList()+@"
    }
";
    }

     static string toDataList(){
        string _string = "";
        foreach(var attribute in attributes){
            if(!attribute.isPrimitive){
                if(attribute.isList){
                    _string += System.Environment.NewLine
+ $"        List<{attribute.type}Data> {attribute.name} = new List<{attribute.type}Data>();" + System.Environment.NewLine
+ $"        foreach(var element in this.{attribute.name}){{" + System.Environment.NewLine
+ $"            {attribute.name}.Add(element.ToData()" + @");
        }
";              
                }
                else{
                    _string += System.Environment.NewLine
+ $"        var {attribute.name} = new {attribute.type}Data({attribute.name})";
                    
                }
            }
        }
        return _string;
     }

     static string fromDataList(string beforeAttribute = ""){
        string _string = "";
        foreach(var attribute in attributes){
            if(!attribute.isPrimitive){
                if(attribute.isList){
                    _string += System.Environment.NewLine
+ $"        foreach(var element in {attribute.name}){{" + System.Environment.NewLine
+ $"            var spawned = Instantiate(PrefabManager.instance.GetPrefab(element.selectedPrefabId), element.position, element.rotation);" + @"
"+ $"            var script = "+ (attribute.type.Equals("Nature") ? "spawned.AddComponent<LoadedNature>();" : $"spawned.GetComponent<{attribute.type}>();")  + @"
"+ @"            script.FromData(element);
        }
";              
                }
                else{
                    _string += System.Environment.NewLine
+ $"        var {attribute.name} = new {attribute.type}Data({attribute.name})";
                    
                }
            }
            else{
                {
                string attriButeName = attribute.name.Equals("position") || attribute.name.Equals("rotation") ? $"transform.{attribute.name}" : attribute.name;
            
                string attributeString =
    $"           this.{attriButeName} = {beforeAttribute}{attribute.name};";
                _string += System.Environment.NewLine + attributeString;
            }
            }
        }
        return _string;
     }

     static string writeToDataClass(string className){
        return 
@"    
"+$"    public {className}Data ToData(){{"+@"
"+$"         return new {className}Data("+ attributesWithoutType() +@");
    }
";
     }

     static string writeFromDataClass(string className){
        return 
@"    
"+$"    public void FromData({className}Data data){{"+@"
        "+fromDataList("data.")+@"
    }
";
     }
}
