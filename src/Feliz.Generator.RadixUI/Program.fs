
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open AngleSharp

type SubComponentParts = { Name: string; Description: string; Props: IWebElement list }

type Prop = { Name: string; Required: bool; Description: string; DataType: string }

type SubComponent = { Name: string; Description: string; Props: Prop list }

type RadixComponent = { Name: string; Description: string; ImportCommand: string; SubComponents: SubComponent list }

let safeFindElement by (element: ISearchContext) =
    try Some (element.FindElement(by))
    with | :? NoSuchElementException -> None

let getExpectedSubcomponents (driver: IWebDriver) =
    driver.FindElements(By.CssSelector("aside li[data-level='3'] a"))
    |> Seq.map (fun elem -> elem.Text)
    |> Seq.toList

let getApiReferenceElements driver =
    let expectedSubcomponents = getExpectedSubcomponents driver
    printfn "%A" expectedSubcomponents
    let startElem = safeFindElement (By.XPath("//h2[@id='api-reference']")) driver |> Option.get
    let js = driver :> IJavaScriptExecutor
    let mutable elems = []
    let mutable current = js.ExecuteScript("return arguments[0].nextElementSibling;", startElem) :?> IWebElement
    while (current <> null) &&  (current.TagName.ToLower() <> "h2" || List.contains current.Text expectedSubcomponents) do
        elems <- current :: elems
        current <- js.ExecuteScript("return arguments[0].nextElementSibling;", current) :?> IWebElement
    List.rev elems

let groupElements (elements: IWebElement list) =
    let rec loop acc currentGroup (elements: IWebElement list) =
        match elements with
        | [] -> List.rev (currentGroup :: acc)
        | h3 :: p :: div :: rest when h3.TagName = "h3" && p.TagName = "p" && div.GetAttribute("class") = "rt-Box rt-r-my-4" ->
            let currentGroup: SubComponentParts =
                { Name = h3.Text; Description = p.Text; Props = [div] }
            loop (currentGroup :: acc) currentGroup rest

        | _ :: rest -> loop acc currentGroup rest

    loop [] { Name = ""; Description = ""; Props = [] } elements

let waitForElement condition (driver: IWebDriver) timeoutSeconds =
    let wait = WebDriverWait(driver, TimeSpan.FromSeconds(float timeoutSeconds))
    wait.Until(condition)

let elementIsVisible (by: By) (driver: IWebDriver) =
    let wait = WebDriverWait(driver, TimeSpan.FromSeconds(30.0))
    wait.Until(fun drv ->
        let element = drv.FindElement(by)
        element.Displayed)

let invisibilityOfElementLocated(by: By) =
    fun (driver: IWebDriver) ->
        try
            let element = driver.FindElement(by)
            not element.Displayed
        with
        | :? NoSuchElementException -> true

let getMainComponentName (driver: IWebDriver) =
    safeFindElement (By.XPath("//h1")) driver
    |> Option.map (fun e -> e.Text)
    |> Option.defaultValue ""

let getMainComponentDescription (driver: IWebDriver) =
    driver
    |> safeFindElement (By.XPath("//*[@id=\"__next\"]/div/div[2]/div[2]/section/div[1]/p[1]"))
    |> Option.map (fun e -> e.Text)
    |> Option.defaultValue ""

let getImportCommand (driver: IWebDriver) =
    driver
    |> safeFindElement (By.XPath("//*[@id=\"__next\"]/div/div[2]/div[2]/section/div[1]/div[5]/pre/code/div"))
    |> Option.map (fun e -> e.Text)
    |> Option.defaultValue ""

let getSubComponentParts = getApiReferenceElements >> groupElements

let processSubComponentPropTable driver (parts: SubComponentParts) =
    parts.Props
    |> List.head
    |> fun e ->
        let rows =  e.FindElements(By.CssSelector("tr.rt-TableRow")) |> Seq.toList

        rows
        |> List.skip 1 // skip header
        |> List.map (fun r ->
            let propNameSection = r.FindElement(By.CssSelector("th.rt-TableCell.rt-TableRowHeaderCell"))
            let propTypeSection = r.FindElement(By.CssSelector("td.rt-TableCell"))

            let propName = propNameSection.FindElement(By.TagName("code")).Text

            let propDescriptionButton = propNameSection.FindElement(By.TagName("button"))
            if propDescriptionButton.Displayed && propDescriptionButton.Enabled then
                propDescriptionButton.Click()

            waitForElement (elementIsVisible(By.CssSelector("div > div.radix-themes.rt-PopperContent.rt-PopoverContent.radix-themes-custom-fonts.rt-r-size-2"))) driver 30 |> ignore

            let propNameDescription = driver.FindElement(By.CssSelector("div > div.radix-themes.rt-PopperContent.rt-PopoverContent.radix-themes-custom-fonts.rt-r-size-2")).Text

            let propTypeDescriptionButton = propNameSection.FindElement(By.TagName("button"))
            if propTypeDescriptionButton.Displayed && propTypeDescriptionButton.Enabled then
                propTypeDescriptionButton.Click()

            waitForElement (invisibilityOfElementLocated(By.CssSelector("div > div.radix-themes.rt-PopperContent.rt-PopoverContent.radix-themes-custom-fonts.rt-r-size-2"))) driver 30 |> ignore

            let propType = propTypeSection.FindElement(By.TagName("code")).Text

            let propTypes =
                match propType with
                | "function" | "enum" ->
                    let descriptionbutton = propTypeSection.FindElement(By.TagName("button"))
                    if descriptionbutton.Displayed && descriptionbutton.Enabled then
                        descriptionbutton.Click()

                    waitForElement (elementIsVisible(By.CssSelector("div > div.radix-themes.rt-PopperContent.rt-PopoverContent > code"))) driver 30 |> ignore

                    let value = driver.FindElement(By.CssSelector("div > div.radix-themes.rt-PopperContent.rt-PopoverContent > code")).Text

                    let descriptionbutton = propNameSection.FindElement(By.TagName("button"))
                    if descriptionbutton.Displayed && descriptionbutton.Enabled then
                        descriptionbutton.Click()

                    waitForElement (invisibilityOfElementLocated(By.CssSelector("div > div.radix-themes.rt-PopperContent.rt-PopoverContent > code"))) driver 30 |> ignore

                    value
                | "boolean" -> "bool"
                | other -> other

            {
                Name = propName.Replace("*", "")
                Required = propName.EndsWith("*")
                Description = propNameDescription.Replace("\n", " ")
                DataType = propTypes
            }
        )

let getRadixComponent (driver: WebDriver) url =
    driver.Url <- url

    let processedSubComponents = Collections.Generic.HashSet<string>()

    let mkSubComponent driver (parts: SubComponentParts) =
        if processedSubComponents.Contains(parts.Name) then
            printfn "Skipping already processed subcomponent: %s" parts.Name
            None
        else
            printfn "Processing subcomponent: %s" parts.Name
            processedSubComponents.Add(parts.Name) |> ignore
            Some {
                Name = parts.Name
                Description = parts.Description
                Props = processSubComponentPropTable driver parts
            }

    {
        Name = getMainComponentName driver
        Description = getMainComponentDescription driver
        ImportCommand = getImportCommand driver |> fun s -> s.Replace("npm install ", "")
        SubComponents = getSubComponentParts driver |> List.choose (mkSubComponent driver)
    }

let getValidComponentUrls () = async {
    let url = "https://www.radix-ui.com/primitives/docs/components/accordion"
    let className = "DocsNav_DocsNavItem__VrHf6"
    let baseUri = new Uri(url)
    let config = Configuration.Default.WithDefaultLoader()
    let! document = Async.AwaitTask (BrowsingContext.New(config).OpenAsync(url))
    let elements = document.QuerySelectorAll($"a.{className}")

    let invalidUrls =
        [
            "https://www.radix-ui.com/primitives/docs/utilities/accessible-icon"
            "https://www.radix-ui.com/primitives/docs/utilities/direction-provider"
            "https://www.radix-ui.com/primitives/docs/utilities/portal"
            "https://www.radix-ui.com/primitives/docs/utilities/slot"
            "https://www.radix-ui.com/primitives/docs/utilities/visually-hidden"
            "https://www.radix-ui.com/primitives/docs/overview/introduction"
            "https://www.radix-ui.com/primitives/docs/overview/getting-started"
            "https://www.radix-ui.com/primitives/docs/overview/accessibility"
            "https://www.radix-ui.com/primitives/docs/overview/releases"
            "https://www.radix-ui.com/primitives/docs/guides/styling"
            "https://www.radix-ui.com/primitives/docs/guides/animation"
            "https://www.radix-ui.com/primitives/docs/guides/composition"
            "https://www.radix-ui.com/primitives/docs/guides/server-side-rendering"
        ]

    return
        elements
        |> Seq.choose (fun element ->
            match element.GetAttribute("href") with
            | null ->
                printfn "Element does not have a href attribute."
                None
            | href when element.QuerySelector("span[data-accent-color='yellow']") <> null ->
                printfn "Skipping deprecated component."
                None
            | href ->
                let absoluteUri = new Uri(baseUri, href)
                Some absoluteUri.AbsoluteUri)
        |> Seq.filter (fun url -> not (invalidUrls |> Seq.contains url))
        |> Seq.toList
}

let refresh () =
    async {
        let! validUrls =  getValidComponentUrls ()

        let options = ChromeOptions()
        options.AddArgument("--headless")

        use driver = new ChromeDriver(options)

        let radixComponents =
            validUrls
            |> List.map (fun (url: string) ->

                printfn "%A" url
                let n = url.Split('/').[6]
                printfn "Processing %s" n
                let comp = getRadixComponent driver url
                System.IO.File.WriteAllText($"components_json/{n}.json", Newtonsoft.Json.JsonConvert.SerializeObject(comp))
                comp
            )

        driver.Quit()
    }


[<EntryPoint>]
let main args =
    async {
        if args[0] = "--refresh" then
            return! refresh()

        return 0
    }
    |> Async.RunSynchronously
