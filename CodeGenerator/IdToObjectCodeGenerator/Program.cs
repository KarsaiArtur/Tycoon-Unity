// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Runtime.InteropServices;



public class MyProgram{

    static public string[] animalVisitables = {"WaterTrough", "AnimalFood"};

    class Attribute{
        public string type;
        public string name;
        public bool isList;

        public Attribute(string type, string name, bool isList){
            this.type = type;
            this.name = name;
            this.isList = isList;
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

        if(!fileContent.Contains("/////GENERATE")){
            return;
        }

        string generatedCode = "////GENERATED" + System.Environment.NewLine;
        System.Console.WriteLine(path);
        foreach(var index in AllIndexesOf(fileContent, "/////GENERATE")){
            var temp = fileContent.Substring(index + "/////GENERATE".Length);
            temp = temp.Substring(temp.IndexOf("private ") + "private ".Length);
            temp = temp[..temp.IndexOf(";")];
            var split = temp.Split(" ");
            
            var name = split[1];
            var isList = false;
            var type = "";

            if(split[0].Contains("List")){
                isList = true;
                type = split[0].Replace("List<", "").Replace(">", "");
            }
            else{
                type = split[0];
            }
            var attribute = new Attribute(type, name, isList);
            generatedCode += writeIdAttribute(attribute);
            if(!attribute.isList){
                generatedCode += writeGetFunction(attribute);
            } else{
                generatedCode += writeGetListFunction(attribute);
                generatedCode += writeAddListFunction(attribute);
                generatedCode += writeRemoveListFunction(attribute);
            }
        }
        
        string end;
        string beforeCode;

        if(!fileContent.Contains("Saveable")){
            if(fileContent.Contains("////GENERATED")){
                beforeCode = fileContent.Substring(0, fileContent.LastIndexOf("////GENERATED"));
            } else{
                beforeCode = fileContent.Substring(0, fileContent.LastIndexOf("}"));
            }
            end = fileContent.Substring(fileContent.LastIndexOf("}"));
        } else{
            if(fileContent.Contains("////GENERATED")){
                beforeCode = fileContent.Substring(0, fileContent.LastIndexOf("////GENERATED"));
            } else{
                beforeCode = fileContent.Substring(0, fileContent.LastIndexOf("///******************************"));;
            }
            end = fileContent.Substring(fileContent.IndexOf("///******************************"));
        }

       
        string newContent = beforeCode + generatedCode + end;
        File.WriteAllText(path, newContent);
        
        return;
    }

    static string writeGetFunction(Attribute attribute){
        var name = attribute.name[0].ToString().ToUpper() + attribute.name[1..];
        var managerList = attribute.type.ToLower() + "List";
        var getString = animalVisitables.Contains(attribute.type) ? $"({attribute.type})AnimalVisitableManager.instance.animalvisitableList.Where((e) => e.GetId() == {attribute.name}Id).FirstOrDefault()"
        : $"{attribute.type}Manager.instance.{managerList}.Where((e) => e.GetId() == {attribute.name}Id).FirstOrDefault()";

return $"    public {attribute.type} Get{name}(string id = null)"+@"
    {
"+$"        id ??={attribute.name}Id;"+@"

"+$"        if(id != {attribute.name}Id || {attribute.name} == null)"+@"
        {
"+$"            {attribute.name}Id = id;"+@"
"+$"            {attribute.name} = {attribute.type}Manager.instance.{managerList}.Where((element) => element.GetId() == {attribute.name}Id).FirstOrDefault();"+@"
        }
"+$"        return {attribute.name};" +@"
    }
";
    }
    static string writeIdAttribute(Attribute attribute){
        return attribute.isList ? System.Environment.NewLine+$"    public List<string> {attribute.name}Ids = new List<string>();"+System.Environment.NewLine
                : System.Environment.NewLine+$"    public string {attribute.name}Id;"+System.Environment.NewLine;
    }

    static string writeGetListFunction(Attribute attribute){
        var name = attribute.name[0].ToString().ToUpper() + attribute.name[1..];
        var managerList = attribute.type.ToLower() + "List";
        var addString = animalVisitables.Contains(attribute.type) ? $"({attribute.type})AnimalVisitableManager.instance.animalvisitableList.Where((e) => e.GetId() == element).FirstOrDefault()"
        : $"{attribute.type}Manager.instance.{managerList}.Where((e) => e.GetId() == element).FirstOrDefault()";
return $"    public List<{attribute.type}> Get{name}()"+@"
    {
"+$"        if({attribute.name} == null)"+@"
        {
"+$"             {attribute.name} = new List<{attribute.type}>();"+@"
"+$"             foreach(var element in {attribute.name}Ids){{"+@"
"+$"                {attribute.name}.Add("+addString+@");
"+$"             }}"+@"
        }
"+$"        return {attribute.name};" +@"
    }
";
    }

    static string writeAddListFunction(Attribute attribute){
        var name = attribute.name[0].ToString().ToUpper() + attribute.name[1..];
        var type = attribute.type.ToLower();
return $"    public void Add{name}({attribute.type} {type})"+@"
    {
"+$"        {attribute.name}Ids.Add({type}.GetId());"+@"
"+$"        Get{name}();"+@"
"+$"        {attribute.name}.Add({type});" +@"
    }
";
    }

    static string writeRemoveListFunction(Attribute attribute){
        var name = attribute.name[0].ToString().ToUpper() + attribute.name[1..];
        var type = attribute.type.ToLower();
return $"    public void Remove{name}({attribute.type} {type})"+@"
    {
"+$"        {attribute.name}Ids.Remove({type}.GetId());"+@"
"+$"        Get{name}();"+@"
"+$"        {attribute.name}.Remove({type});" +@"
    }
";
    }

        /*

    generatedCode += writeDataClass(className);
    generatedCode += writeDataToJson(className);
    generatedCode += writeFromJson(className);
    string fileName = path.Substring(path.LastIndexOf("\\") + 1);
    fileName = fileName.Substring(0, fileName.IndexOf(".cs")) +".json";
    generatedCode += writeGetFileName(fileName);
    generatedCode += writeSetData();
        generatedCode += writeToDataClass(className);
        generatedCode += writeFromDataClass(className);

        string editedCode = fileContent.Substring(0, fileContent.LastIndexOf("}"));

        
        if(editedCode.Contains("///******************************")){
                editedCode = editedCode.Substring(0, editedCode.IndexOf("///******************************"));
        }

        string newContent = editedCode + generatedCode + end;

        File.WriteAllText(path, newContent);
    }

    static string attributesWithType(){
        var finalAttribute = attributes[0].type;
        finalAttribute = attributes[0].isList ? $"List<{finalAttribute}>" : finalAttribute;
        string _string = $"{finalAttribute} {attributes[0].name}";
        foreach(var attribute in attributes.Skip(1)){
            finalAttribute = attribute.type;
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
$"    public class {className}Data"+@"
    {";
        foreach(var attribute in attributes){
            string finalAttribute = "";
            finalAttribute = attribute.type switch
            {
                "DateTime" => "long",
                _ => attribute.type,
            };
            finalAttribute = finalAttribute;
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
                if(attribute.isList){
                    _string += System.Environment.NewLine
+ $"        List<{attribute.type}Data> {attribute.name} = new List<{attribute.type}Data>();" + System.Environment.NewLine
+ $"        foreach(var element in this.{attribute.name}){{" + System.Environment.NewLine
+ $"            {attribute.name}.Add(element.ToData()" + @");
        }
";              
                    _string += System.Environment.NewLine
+ $"        var {attribute.name} = new {attribute.type}Data({attribute.name})";
                    
            }
        }
        return _string;
     }

     static string fromDataList(string beforeAttribute = ""){
        string _string = "";
        foreach(var attribute in attributes){
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
                string attriButeName = attribute.name.Equals("position") || attribute.name.Equals("rotation") ? $"transform.{attribute.name}" : attribute.name;
            
                string attributeString =
    $"           this.{attriButeName} = {beforeAttribute}{attribute.name};";
                _string += System.Environment.NewLine + attributeString;
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
     }*/

    public static int[] AllIndexesOf(string str, string substr, bool ignoreCase = false)
    {
        var indexes = new List<int>();
        int index = 0;

        while ((index = str.IndexOf(substr, index, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) != -1)
        {
            indexes.Add(index++);
        }

        return indexes.ToArray();
    }
}

