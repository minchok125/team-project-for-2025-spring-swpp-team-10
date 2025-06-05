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
        // string result = input;
        //
        // foreach (var pair in _colorCode)
        // {
        //     string opStart = "$" + pair.Key + "$";
        //     string opEnd = "$\\" + pair.Key + "$";
        //     result = result.Replace(opStart, $"<color=#{pair.Value}>");
        //     result = result.Replace(opEnd, $"</color>");
        // }


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
                if (input[idx + 2] == '$')
                {
                    _sb.Append("<color=#");
                    char op = char.ToLower(input[idx + 1]); 
                    switch (op)
                    {
                        case 'd': case 'r': case 'o': case 'y': case 'g': case 'b': case 'p':
                            _sb.Append(_colorCode[op] + ">");
                            idx += 3;  
                            break;
                
                        default:
                            _sb.Append(input[idx]); idx++;  break;
                    }
                }
                else if (input[idx + 1] == '/' && input[idx + 3] == '$') 
                { _sb.Append("</color>"); idx += 4; }
                else { _sb.Append(input[idx]); idx++; }
                
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
