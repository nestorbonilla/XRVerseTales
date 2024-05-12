using System;
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
    private string _epubFilePath;
    private EpubBook _book;
    private List<string> _pages;
    private int _currentPageIndex = 0;
    private StringBuilder _sb;
    private bool _introductionFound = false;
    private int _introductionIndex = -1;

    public TextMeshPro textMeshPro;
    
    void Start()
    {
        _epubFilePath = Path.Combine(Application.dataPath, "Books", _epubFileName);

        if (File.Exists(_epubFilePath))
        {
            _book = ReadEpubBook(_epubFilePath);
            Debug.Log("BM: Title: " + _book.Title);
            LoadPages();
            RenderChapter(1);
        }
        else
        {
            Debug.LogError("BM: EPUB file not found at: " + _epubFilePath);
        }
    }

    private EpubBook ReadEpubBook(string filePath)
    {
        return EpubReader.ReadBook(filePath);
    }

    private void LoadPages()
    {
        bool startRendering = false;

        _pages = new List<string>();
        foreach (EpubLocalTextContentFile textContentFile in _book.ReadingOrder)
        {
            if (!string.IsNullOrEmpty(textContentFile.Content))
            {
                string[] chapterPages = DivideIntoPages(textContentFile.Content);
                if (!startRendering)
                {
                    foreach (string page in chapterPages)
                    {
                        // Let's start from the first chapter
                        if (page.Contains("I.\nIntroduction"))
                        {
                            startRendering = true;
                            _pages.Add(page);
                            break;
                        }
                    }
                }
                else
                {
                    _pages.AddRange(chapterPages);
                }
            }
        }
    }
    private string[] DivideIntoPages(string htmlContent)
    {
        string cleanedHtml = CleanHtml(htmlContent);
        string[] pageArray = cleanedHtml.Split(new string[] { "<p>", "</p>" }, System.StringSplitOptions.RemoveEmptyEntries);
        return pageArray;
    }

    private string CleanHtml(string htmlContent)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        string innerText = doc.DocumentNode.InnerText;

        // On all inner texts, there's a repeating text that we need to remove
        // We can use the index of the first occurrence of the text to remove it
        if (_introductionIndex == -1)
        {
            _introductionIndex = innerText.IndexOf("I.\nIntroduction");
        }

        if (_introductionIndex != -1)
        {
            innerText = innerText.Substring(_introductionIndex);
        }

        return innerText.Trim();
    }

    public void RenderChapter(int pageNumber)
    {
        int pageIndex = pageNumber - 1;
        if (pageIndex >= 0 && pageIndex < _pages.Count)
        {
            string visibleText = GetVisibleText(_pages[pageIndex]);
            JSONObject requestData = new JSONObject();
            requestData.Add("BM: text", visibleText);
            string name = _epubFileName + "_" + pageNumber.ToString();
            aIManager.PaintBackgroundImage(requestData, name);
            textMeshPro.text = visibleText;
        }
        else
        {
            Debug.LogError("BM: Page index out of range");
        }
    }

    private string GetVisibleText(string htmlContent)
    {
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);
        StringBuilder sb = new StringBuilder();
        foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//text()"))
        {
            string trimmedText = node.InnerText.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedText))
            {
                sb.AppendLine(trimmedText);
            }
        }
        return sb.ToString();
    }
}