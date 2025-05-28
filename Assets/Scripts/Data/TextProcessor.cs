using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Analytics;

public class TextProcessor
{
    private Dictionary<char, string> _colorCode = new Dictionary<char, string>();

    public TextProcessor(Color d, Color r, Color o, Color y, Color g, Color b, Color p)
    {
        _colorCode.Add('d', ColorUtility.ToHtmlStringRGBA(d));
        _colorCode.Add('r', ColorUtility.ToHtmlStringRGBA(r));
        _colorCode.Add('o', ColorUtility.ToHtmlStringRGBA(o));
        _colorCode.Add('y', ColorUtility.ToHtmlStringRGBA(y));
        _colorCode.Add('g', ColorUtility.ToHtmlStringRGBA(g));
        _colorCode.Add('b', ColorUtility.ToHtmlStringRGBA(b));
        _colorCode.Add('p', ColorUtility.ToHtmlStringRGBA(p));
    }
    
    public string Process(string input)
    {
        Queue<string> queue = new Queue<string>();
        StringBuilder _sb = new StringBuilder();
        _sb.Clear();

        int idx = 0;
        while (idx < input.Length)
        {
            if (input[idx] != '$')
            { _sb.Append(input[idx]); idx++; continue; }

            try
            {
                char op;
                bool end = false;
                
                if (input[idx+2] == '$') { _sb.Append("<color=#"); op = char.ToLower(input[idx + 1]); }
                else if (input[idx + 1] == '\\' && input[idx + 3] == '$') 
                { _sb.Append("<\\color=#"); op = char.ToLower(input[idx + 2]); end = true; }
                else { _sb.Append(input[idx]); idx++; continue; }
                
                switch (op)
                {
                    case 'd': case 'r': case 'o': case 'y': case 'g': case 'b': case 'p':
                        _sb.Append(_colorCode[op] + ">");
                        idx += 3; 
                        if (end) idx++; 
                        break;
                
                    default:
                        _sb.Append(input[idx]); idx++;  break;
                }
            }
            catch
            {
                if (idx < input.Length) _sb.Append(input.Substring(idx));
                break;
            }
        }
        return _sb.ToString();
    }
}
