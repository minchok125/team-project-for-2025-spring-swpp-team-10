using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TextProcessor
{
    private StringBuilder _sb = new StringBuilder();

    private Dictionary<char, string> _colorCode = new Dictionary<char, string>();

    public TextProcessor(Color d, Color r, Color o, Color y, Color g, Color b, Color p)
    {
        _colorCode.Add('d', ColorUtility.ToHtmlStringRGB(d));
        _colorCode.Add('r', ColorUtility.ToHtmlStringRGB(r));
        _colorCode.Add('o', ColorUtility.ToHtmlStringRGB(o));
        _colorCode.Add('y', ColorUtility.ToHtmlStringRGB(y));
        _colorCode.Add('g', ColorUtility.ToHtmlStringRGB(g));
        _colorCode.Add('b', ColorUtility.ToHtmlStringRGB(b));
        _colorCode.Add('p', ColorUtility.ToHtmlStringRGB(p));
    }
    
    public string Process(string input)
    {
        _sb.Clear();

        int idx = 0;
        while (idx < input.Length)
        {
            if (input[idx] != '$')
            { _sb.Append(input[idx]); idx++; continue; }
            
            char op = char.ToLower(input[idx + 1]);

            switch (op)
            {
                case 'd': case 'r': case 'o': case 'y': case 'g': case 'b': case 'p':
                    if (input[idx + 2] != '$')
                    { _sb.Append(input, idx, idx+2); idx += 3; continue; }

                    string endOp = "$" + op + "\\$";
                    if (!input.Contains(endOp, StringComparison.OrdinalIgnoreCase))
                    { _sb.Append(input, idx, idx+2); idx += 3; continue; }
                    
                    _sb.Append($"<color={_colorCode[op]}>");
                    input = input.Replace(endOp, $"<\\color={_colorCode[op]}>");
                    idx += 2;
                    
                    break;
                
                default:
                    _sb.Append(input, idx, idx+1); idx += 2; break;
            }
        }

        return _sb.ToString();
    }
}
