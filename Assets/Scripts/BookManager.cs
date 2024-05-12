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
    private string _epubFileName = "the-time-machine.epub";
    private string epubFilePath;
    private EpubBook book;
    private List<string> pages;
    private int currentPageIndex = 0;

    public TextMeshPro textMeshPro;
    
    void Start()
    {
        epubFilePath = Path.Combine(Application.dataPath, "Books", _epubFileName);

        if (File.Exists(epubFilePath))
        {
            book = ReadEpubBook(epubFilePath);
            // Debug.Log("Title: " + book.Title);
            // Debug.Log("Author: " + book.Author);
            LoadPages();
            // RenderPage(5);
        }
        else
        {
            Debug.LogError("The EPUB file was not found at the specified path: " + epubFilePath);
        }
    }

    private EpubBook ReadEpubBook(string filePath)
    {
        return EpubReader.ReadBook(filePath);
    }

    private void LoadPages()
    {
        foreach (EpubLocalTextContentFile textContentFile in book.ReadingOrder)
        {
            PrintTextContentFile(textContentFile);
        }
        
        // pages = new List<string>();
        // foreach (EpubLocalTextContentFile textContentFile in book.ReadingOrder)
        // {
        //     if (!string.IsNullOrEmpty(textContentFile.Content))
        //     {
        //         string[] chapterPages = DivideIntoPages(textContentFile.Content);
        //         pages.AddRange(chapterPages);
        //     }
        // }
    }
    
    private static void PrintTextContentFile(EpubLocalTextContentFile textContentFile)
    {
        HtmlDocument htmlDocument = new();
        htmlDocument.LoadHtml(textContentFile.Content);
        int sectionCount = htmlDocument.DocumentNode.SelectNodes("//text()").Count;
        StringBuilder sb = new();
        for (int x = 0; x < sectionCount; x++)
        {
            HtmlNode node = htmlDocument.DocumentNode.SelectNodes("//text()")[x];
            if (x>2)
            {
                sb.AppendLine(node.InnerText.Trim());
                string contentText = sb.ToString();
                Debug.Log("Content of file " + contentText);
                Debug.Log("______________________________");
            }
        }
        // foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//text()"))
        // {
        //     sb.AppendLine(node.InnerText.Trim());
        // }
        
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
            string name = _epubFileName + "_" + pageNumber.ToString();
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