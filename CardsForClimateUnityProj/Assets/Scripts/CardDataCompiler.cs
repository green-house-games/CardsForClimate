using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

// Most of this class is adapted from the following open-source code: 
// https://gist.github.com/ppolsinelli/a3bf42380e6287a2cf6e67c3e29d58ec#file-googlesheetdownload-cs

/// <summary>
/// CardDataCompiler downloads card information from the card spreadsheet via Google Sheets, then formats it in a
/// way that is more easily understood by the rest of our code.
/// </summary>
public class CardDataCompiler : MonoBehaviour
{
    /// <summary>
    /// Reference list of event cards for event deck queue in the GameManager to pull from
    /// </summary>
    public List<EventCard> MasterEventDeck { get; private set; } = new List<EventCard>();

    /// <summary>
    /// Reference lists of action cards for action deck queue in the GameManager to pull from
    /// </summary>
    public List<ActionCard> MasterActionDeck { get; private set; } = new List<ActionCard>();

    /// <summary>
    /// Dictionary of possible catastrophe cards, keyed by the cards that cause them
    /// </summary>
    public Dictionary<string, EventCard> SuperNegativeEventCards { get; private set; } = new Dictionary<string, EventCard>();

    /// <summary>
    /// List of positive event cards
    /// </summary>
    public List<EventCard> PositiveEventCards { get; private set; } = new List<EventCard>();

    /// <summary>
    /// Singleton instance of the CardDataCompiler for other classes to access
    /// </summary>
    public static CardDataCompiler Instance;

    private void Awake()
    {
        if (Instance != null) Debug.LogError("More than one instance of CardSpreadsheetPuller");
        Instance = this;
    }

    /// <summary>
    /// Called before the first frame update
    /// </summary>
    void Start()
    {
        PullCardSpreadsheet();
    }

    public static readonly string DocumentId = "1vvL7R6ShXb3P8KQ4I1YoglJB3Ugz296hbsG3lyh_Obs";
    public static readonly string CSVName = "CardTable";
    public void PullCardSpreadsheet()
    {
        /*if (Application.isEditor)
        {
            Action<string> commCallback = (csv) => {
                LoadCSVText(csv);
            };
            StartCoroutine(DownloadSpreadsheet(DocumentId, commCallback, true, CSVName));
        } else
        {*/
            var data = Resources.Load(CSVName) as TextAsset;
            LoadCSVText(data.text);
        //}
    }

    /// <summary>
    /// Contacts Google Sheets and downloads the spreadsheet of card information as a CSV file
    /// </summary>
    /// <param name="docId">The document ID, used in the URL to request the proper spreadsheet</param>
    /// <param name="callback">The function to call once the spreadsheet is acquired</param>
    /// <param name="saveAsset">Should we save the spreadsheet as a local asset?</param>
    /// <param name="assetName">If we're saving the spreadsheet, this is the local asset's file name</param>
    /// <param name="sheetId">The specific sheet we want to access within the spreadsheet file</param>
    /// <returns></returns>
    public static IEnumerator DownloadSpreadsheet(string docId, Action<string> callback, bool saveAsset = false, 
                                                   string assetName = null, string sheetId = null)
    {
        string url = "https://docs.google.com/spreadsheets/d/" + docId + "/export?format=csv";
        if (!string.IsNullOrEmpty(sheetId)) url += "&gid=" + sheetId;
        Debug.Log("Downloading spreadsheet...");
        using (UnityWebRequest downloadRequest = UnityWebRequest.Get(url))
        {
            yield return downloadRequest.SendWebRequest();

            if (downloadRequest.isNetworkError)
            {
                Debug.Log("Error downloading: " + downloadRequest.error);
            } else
            {
                Debug.Log("Spreadsheet downloaded!");
                callback(downloadRequest.downloadHandler.text);
                if (saveAsset)
                {
                    if (!string.IsNullOrEmpty(assetName))
                    {
                        // Cleans up the spreadsheet so it can safely enter the parsing process
                        string cleanText = CleanReturnInCsvTexts(downloadRequest.downloadHandler.text);
                        File.WriteAllText("Assets/Resources/" + assetName + ".csv", cleanText);
                        Debug.Log("File written");
                    } else
                    {
                        throw new Exception("assetName is null");
                    }
                }
            }
        }
    }

    /// <summary>
    /// Encodes and cleans up special characters in the CSV that would interfere with the parsing process 
    /// if left untouched
    /// </summary>
    /// <param name="text">The raw CSV string to clean up</param>
    /// <returns>A cleaned CSV string with certain special characters encoded</returns>
    public static string CleanReturnInCsvTexts(string text)
    {
        text = text.Replace("\"\"", "'");

        if (text.IndexOf("\"") > -1)
        {
            string cleanText = "";
            bool insideQuote = false;
            for (int j = 0; j < text.Length; j++)
            {
                if (!insideQuote && text[j] == '\"') // signals beginning of quote
                {
                    insideQuote = true;
                } else if (insideQuote && text[j] == '\"') // signals end of quote
                {
                    insideQuote = false;
                } else if (insideQuote)
                {
                    if (text[j] == '\n') cleanText += "<br>"; // encode line break characters
                    else if (text[j] == ',') cleanText += "<c>"; // encode comma characters
                    else cleanText += text[j];
                } else
                {
                    cleanText += text[j];
                }
            }
            text = cleanText;
        }
        return text;
    }

    /// <summary>
    /// Loads all card data from a large string containing the info in CSV format.
    /// </summary>
    /// <param name="csv">The string containing card info in CSV format</param>
    public void LoadCSVText(string csv)
    {
        Debug.Log("Loading CSV Text...");
        // Parses CSV into a format where each line is separated and split further into a list of cells
        List<List<string>> parsedCsv = ParseCSV(csv);
        foreach (List<string> line in parsedCsv)
        {
            Card thisCard;
            if (line[1].ToLower() == "action")
            { // this is an Action Card
                thisCard = new ActionCard();
                MasterActionDeck.Add((ActionCard)thisCard);
            } else if (line[1].ToLower() == "event")
            { // this is an Event Card
                thisCard = new EventCard();
                if (int.Parse(line[10]) == 1)
                { // this is a super positive event card
                    PositiveEventCards.Add((EventCard)thisCard);
                } else if (int.Parse(line[11]) == 1)
                { // this is a super negative event card
                    SuperNegativeEventCards.Add(line[12], (EventCard)thisCard);
                } else
                { // this is just a normal event card
                    MasterEventDeck.Add((EventCard)thisCard);
                }
            } else
            { // We don't recognize this card's type
                Debug.LogError("Unknown card type " + line[1]);
                continue;
            }

            // Add basic card info from the corresponding slots in the CSV line
            int.TryParse(line[0], out thisCard.cardNumber);
            thisCard.cardName = line[3];
            thisCard.cardDesc = line[5];
            int.TryParse(line[6], out thisCard.costCarbon);
            int.TryParse(line[7], out thisCard.costMoney);
            int.TryParse(line[8], out thisCard.hope);
            int.TryParse(line[9], out thisCard.momentum);

            // Load card image from the corresponding file path for that image
            // e.g., "ActionCards/2-Choose Public Transport"
            thisCard.cardImage = Resources.Load<Sprite>(
                (line[1].ToLower() == "action" ? "ActionCards" : "EventCards") 
                + "/" + thisCard.cardNumber.ToString() + "-" + thisCard.cardName);
        }

        Debug.Log("Done loading CSV!");
        // Cards are all loaded and ready to go; time to pass this off to the GameManager
        GameManager.Instance.SetupGame();
    }

    //CSV reader from https://bravenewmethod.com/2014/09/13/lightweight-csv-reader-for-unity/
    
    /// <summary>
    /// Loads and parses CSV information for cards from a local file
    /// </summary>
    /// <param name="file">The local file to load CSV card data from</param>
    /// <returns>A formatted list of data cell lists for the cards</returns>
    public static List<List<string>> ReadCSV(string file)
    {
        var data = Resources.Load(file) as TextAsset;
        return ParseCSV(data.text);
    }

    // Regex strings used to detect line breaks, data cell split characters, and other special CSV characters
    public static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    public static readonly string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    public static readonly char[] TRIM_CHARS = { '\"' };

    /// <summary>
    /// Turns a raw string containing a CSV-formatted list of cards into a list of lines, where each line is a list
    /// of data cell values
    /// </summary>
    /// <param name="text">The raw string containing a CSV-formatted list of cards</param>
    /// <returns>A formatted list of data cell lists for the cards</returns>
    public static List<List<string>> ParseCSV(string text)
    {
        var parsedList = new List<List<string>>();
        var lines = Regex.Split(text, LINE_SPLIT_RE);

        if (lines.Length <= 1) return parsedList; // If there's nothing but a header, no need to keep parsing

        var header = Regex.Split(lines[0], SPLIT_RE);

        bool jumpedFirst = false;

        foreach (var line in lines)
        {
            if (!jumpedFirst)
            { // lets us jump the header info at the top of the CSV to get straight to the data
                jumpedFirst = true;
                continue;
            }
            var values = Regex.Split(line, SPLIT_RE);

            var entry = new List<string>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                var value = values[j];
                value = DecodeSpecialCharsFromCSV(value);
                entry.Add(value);
            }
            parsedList.Add(entry);
        }
        return parsedList;
    }

    /// <summary>
    /// Takes some special characters, used as intermediaries during the CSV-parsing process, and turns them back into
    /// their intended forms
    /// </summary>
    /// <param name="value">The data cell to decode special characters from</param>
    /// <returns>The decoded data cell</returns>
    public static string DecodeSpecialCharsFromCSV(string value)
    {
        value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "").Replace("<br>", "\n").Replace("<c>", ",");
        return value;
    }
}
