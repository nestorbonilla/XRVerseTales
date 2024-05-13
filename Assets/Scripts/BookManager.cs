using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    private List<string> _chapters;
    private List<string> _pages;
    private int _charactersPerPage = 1000;
    private int _currentPageIndex = 0;
    private StringBuilder _sb;
    private bool _introductionFound = false;
    private int _introductionIndex = -1;
    public int _pageNumber = 1;

    public TextMeshProUGUI textMeshPro;
    
    void Start()
    {
        _epubFilePath = Path.Combine(Application.dataPath, "Books", _epubFileName);
        if (File.Exists(_epubFilePath))
        {
            _book = ReadEpubBook(_epubFilePath);
            LoadPages();
            RenderPage(1);
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
        LoadChapters();
        _pages = new List<string>();
        foreach (var chapter in _chapters)
        {
            List<string> chapterPages = DivideIntoPages(chapter).ToList();
            if (chapterPages.Count > 0)
            {
                _pages.AddRange(chapterPages);
            }
            else
            {
                Debug.Log("BM: Chapter is empty");
            }
        }
        Debug.Log("BM: Page loaded: " + _pages[0]);
    }
    private void LoadChapters()
    {
        bool startRendering = false;

        _chapters = new List<string>();
        foreach (EpubLocalTextContentFile textContentFile in _book.ReadingOrder)
        {
            if (!string.IsNullOrEmpty(textContentFile.Content))
            {
                string[] chapters = DivideIntoChapters(textContentFile.Content);
                if (!startRendering)
                {
                    foreach (string page in chapters)
                    {
                        // Let's start from the first chapter
                        if (page.Contains("I.\nIntroduction"))
                        {
                            startRendering = true;
                            _chapters.Add(page);
                            break;
                        }
                    }
                }
                else
                {
                    _chapters.AddRange(chapters);
                }
            }
        }
    }
    
    private List<string> DivideIntoPages(string chapter)
    {
        List<string> pages = new List<string>();
        int startIndex = 0;
        while (startIndex < chapter.Length)
        {
            int endIndex = Math.Min(startIndex + _charactersPerPage, chapter.Length);
            string page = chapter.Substring(startIndex, endIndex - startIndex).Trim();
            pages.Add(page);
            startIndex = endIndex;
        }

        return pages;
    }
    
    private string[] DivideIntoChapters(string htmlContent)
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

    public void NextPage()
    {
        if (_pageNumber < _pages.Count)
        {
            _pageNumber++;
            RenderPage(_pageNumber);
        }
    }
    
    public void PreviousPage()
    {
        if (_pageNumber > 1)
        {
            _pageNumber--;
            RenderPage(_pageNumber);
        }
    }
    
    private void RenderPage(int pageNumber)
    {
        int pageIndex = pageNumber - 1;
        if (pageIndex >= 0 && pageIndex < _pages.Count)
        {
            textMeshPro.text = _pages[pageIndex];
            RenderAI(_pages[pageIndex], pageIndex); 
        }
        else
        {
            Debug.LogError("BM: Page index out of range");
        }
    }
    
    private void RenderAI(string page, int pageNumber) {
        JSONObject requestData = new JSONObject();
        requestData.Add("text", page);
        string name = _epubFileName + "_" + pageNumber.ToString();
        aIManager.PaintBackgroundImage(requestData, name);
    }
    
}