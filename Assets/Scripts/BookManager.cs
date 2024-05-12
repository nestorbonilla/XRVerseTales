using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;
using HtmlAgilityPack;
using OVRSimpleJSON;
using VersOne.Epub;

public class BookManager : MonoBehaviour
{
    [SerializeField] public AIManager aIManager;
    public string epubFileName = "principito.epub"; // The name of the EPUB file
    private string epubFilePath; // The full path to the EPUB file
    private EpubBook book; // The EPUB book object
    private List<string> pages; // The list of pages in the book
    private int currentPageIndex = 0; // The current page index

    public TextMeshPro textMeshPro; // The TextMeshPro component

    // Start is called before the first frame update
    void Start()
    {
        // Get the full path of the EPUB file within the Assets folder
        epubFilePath = Path.Combine(Application.dataPath, "Books", epubFileName);

        if (File.Exists(epubFilePath))
        {
            book = ReadEpubBook(epubFilePath);
            // Debug.Log("Title: " + book.Title);
            // Debug.Log("Author: " + book.Author);
            LoadPages();
            RenderPage(5); // Render the first page
        }
        else
        {
            Debug.LogError("The EPUB file was not found at the specified path: " + epubFilePath);
        }
    }

    private EpubBook ReadEpubBook(string filePath)
    {
        return EpubReader.ReadBook(filePath); // Read the EPUB book from the file
    }

    private void LoadPages()
    {
        pages = new List<string>();
        foreach (EpubLocalTextContentFile textContentFile in book.ReadingOrder)
        {
            if (!string.IsNullOrEmpty(textContentFile.Content))
            {
                string[] chapterPages = DivideIntoPages(textContentFile.Content);
                pages.AddRange(chapterPages); // Add the pages to the list
            }
        }
    }

    private string[] DivideIntoPages(string htmlContent)
    {
        // Clean the HTML content
        string cleanedHtml = CleanHtml(htmlContent);

        // Divide the content into pages based on paragraph tags
        string[] pageArray = cleanedHtml.Split(new string[] { "<p>", "</p>" }, System.StringSplitOptions.RemoveEmptyEntries);

        return pageArray;
    }

    private string CleanHtml(string htmlContent)
    {
        // Create an HTML document
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Get the visible text and return it
        return doc.DocumentNode.InnerText.Trim();
    }

    // Function to render a specific page based on the page number
    public void RenderPage(int pageNumber)
    {
        int pageIndex = pageNumber - 1;
        if (pageIndex >= 0 && pageIndex < pages.Count)
        {
            Debug.Log("Rendering page " + pageNumber);
            string visibleText = GetVisibleText(pages[pageIndex]);
            Debug.Log(visibleText);
            JSONObject requestData = new JSONObject();
            requestData.Add("text", visibleText);
            string name = epubFileName + "_" + pageNumber.ToString();
            aIManager.PaintBackgroundImage(requestData, name);

            // Update the text in TextMeshPro
            textMeshPro.text = visibleText;
        }
        else
        {
            Debug.LogError("Page index out of range");
        }
    }

    private string GetVisibleText(string htmlContent)
    {
        // Create an HTML document
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Get the visible text and concatenate it
        StringBuilder sb = new StringBuilder();
        foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//text()"))
        {
            string trimmedText = node.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedText))
            {
                sb.AppendLine(trimmedText);
            }
        }

        // Return the visible text
        return sb.ToString();
    }
}