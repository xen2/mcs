//
// ConditionTokenizer.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//   Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// (C) 2006 Marek Sieradzki
// (C) 2004-2006 Jaroslaw Kowalski
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Microsoft.Build.BuildEngine {

	internal sealed class ConditionTokenizer {
	
		string	inputString = null;
		int	position = 0;
		int	tokenPosition = 0;
		
		Token	token;
		
//		bool	ignoreWhiteSpace = true;
		
		static TokenType[] charIndexToTokenType = new TokenType[128];
		static Dictionary <string, TokenType> keywords = new Dictionary <string, TokenType> (StringComparer.InvariantCultureIgnoreCase);

		static ConditionTokenizer ()
		{
			for (int i = 0; i < 128; i++)
				charIndexToTokenType [i] = TokenType.Invalid;
			
			foreach (CharToTokenType cht in charToTokenType)
				charIndexToTokenType [(int) cht.ch] = cht.tokenType;
			
			keywords.Add ("and", TokenType.And);
			keywords.Add ("or", TokenType.Or);
		}
		
		public ConditionTokenizer ()
		{
//			this.ignoreWhiteSpace = true;
		}
		
		public void Tokenize (string s)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
		
			this.inputString = s;
			this.position = 0;
			this.token = new Token (null, TokenType.BOF);

			GetNextToken ();
		}
		
		void SkipWhiteSpace ()
		{
			int ch;
			
			while ((ch = PeekChar ()) != -1) {
				if (!Char.IsWhiteSpace ((char)ch))
					break;
				ReadChar ();
			}
		}
		
		int PeekChar ()
		{
			if (position < inputString.Length)
				return (int) inputString [position];
			else
				return -1;
		}
		
		int ReadChar ()
		{
			if (position < inputString.Length)
				return (int) inputString [position++];
			else
				return -1;
		}
		
		public void Expect (TokenType type)
		{
			if (token.Type != type)
				throw new ExpressionParseException ("Expected token type of type: " + type + ", got " + token.Type +
					" (" + token.Value + ") .");
			
			GetNextToken ();
		}
		
		public bool IsEOF ()
		{
			return token.Type == TokenType.EOF;
		}
		
		public bool IsNumber ()
		{
			return token.Type == TokenType.Number;
		}
		
		public bool IsToken (TokenType type)
		{
			return token.Type == type;
		}
		
		public bool IsPunctation ()
		{
			return (token.Type >= TokenType.FirstPunct && token.Type < TokenType.LastPunct);
		}
		
		// FIXME: add 'and' and 'or' tokens
		public void GetNextToken ()
		{
			if (token.Type == TokenType.EOF)
				throw new ExpressionParseException ("Cannot read past the end of stream.");
			
			SkipWhiteSpace ();
			
			tokenPosition = position;
			
//			int i = PeekChar ();
			int i = ReadChar ();
			
			if (i == -1) {
				token = new Token (null, TokenType.EOF);
				return;
			}
			
			char ch = (char) i;

			
			// FIXME: looks like a hack: if '-' is here '->' won't be tokenized
			// maybe we should treat item reference as a token
			if (Char.IsDigit (ch) || ch == '-') {
				StringBuilder sb = new StringBuilder ();
				
				sb.Append (ch);
				
				while ((i = PeekChar ()) != -1) {
					ch = (char) i;
					
					if (Char.IsDigit (ch) || ch == '.')
						sb.Append ((char) ReadChar ());
					else
						break;
				}
				
				token = new Token (sb.ToString (), TokenType.Number);
			} else if (ch == '\'') {
				StringBuilder sb = new StringBuilder ();
				string temp;
				
				sb.Append (ch);
				
				while ((i = PeekChar ()) != -1) {
					ch = (char) i;
					
					sb.Append ((char) ReadChar ());
					
					if (ch == '\'')
						break;
				}
				
				temp = sb.ToString ();
				
				token = new Token (temp.Substring (1, temp.Length - 2), TokenType.String);
				
			} else 	if (ch == '_' || Char.IsLetter (ch)) {
				StringBuilder sb = new StringBuilder ();
				
				sb.Append ((char) ch);
				
				while ((i = PeekChar ()) != -1) {
					if ((char) i == '_' || Char.IsLetterOrDigit ((char) i))
						sb.Append ((char) ReadChar ());
					else
						break;
				}
				
				string temp = sb.ToString ();
				
				if (keywords.ContainsKey (temp))
					token = new Token (temp, keywords [temp]);
				else
					token = new Token (temp, TokenType.String);
					
			} else if (ch == '!' && PeekChar () == (int) '=') {
				token = new Token ("!=", TokenType.NotEqual);
				ReadChar ();
			} else if (ch == '<' && PeekChar () == (int) '=') {
				token = new Token ("<=", TokenType.LessOrEqual);
				ReadChar ();
			} else if (ch == '>' && PeekChar () == (int) '=') {
				token = new Token (">=", TokenType.GreaterOrEqual);
				ReadChar ();
			} else if (ch == '=' && PeekChar () == (int) '=') {
				token = new Token ("==", TokenType.Equal);
				ReadChar ();
			} else if (ch >= 32 && ch < 128) {
				if (charIndexToTokenType [ch] != TokenType.Invalid) {
					token = new Token (new String (ch, 1), charIndexToTokenType [ch]);
					return;
				} else
					throw new ExpressionParseException (String.Format ("Invalid punctuation: {0}", ch));
			} else
				throw new ExpressionParseException (String.Format ("Invalid token: {0}", ch));
		}
		
		public int TokenPosition {
			get { return tokenPosition; }
		}
		
		public Token Token {
			get { return token; }
		}
		
/*
		public bool IgnoreWhiteSpace {
			get { return ignoreWhiteSpace; }
			set { ignoreWhiteSpace = value; }
		}
*/
		
		struct CharToTokenType {
			public char ch;
			public TokenType tokenType;
			
			public CharToTokenType (char ch, TokenType tokenType)
			{
				this.ch = ch;
				this.tokenType = tokenType;
			}
		}
		
		static CharToTokenType[] charToTokenType = {
			new CharToTokenType ('<', TokenType.Less),
			new CharToTokenType ('>', TokenType.Greater),
			new CharToTokenType ('=', TokenType.Equal),
			new CharToTokenType ('(', TokenType.LeftParen),
			new CharToTokenType (')', TokenType.RightParen),
			new CharToTokenType ('.', TokenType.Dot),
			new CharToTokenType (',', TokenType.Comma),
			new CharToTokenType ('!', TokenType.Not),
			new CharToTokenType ('@', TokenType.Item),
			new CharToTokenType ('$', TokenType.Property),
			new CharToTokenType ('%', TokenType.Metadata),
			new CharToTokenType ('\'', TokenType.Apostrophe),
		};
	}
}

#endif
