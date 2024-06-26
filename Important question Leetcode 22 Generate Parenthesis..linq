<Query Kind="Program" />

void Main()
{
	// https://leetcode.com/problems/generate-parentheses/description/ backtracking.
	int n = 3;
	this.GenerateParenthesis(3).Dump();
	
}

public IList<string> GenerateParenthesis(int n)
{
	List<char> options = new List<char>() { '(', ')' };
	List<string> results = new List<string>();
	
	GenerateParenthesis_Internal(0,0,new List<char>());
	return results;
	void GenerateParenthesis_Internal(int openBracketCount, int closedBranchCount, List<char> chars)
	{
		if (closedBranchCount > openBracketCount)
		{
			return;
		}
		
		if (chars.Count == n * 2)
		{
			if (openBracketCount == closedBranchCount)
			{
				results.Add(string.Join("", chars));
			}
			
			return;
		}
		
		// BackTracking... 
		foreach (var option in options)
		{
			if (option == '(')
			{
				openBracketCount++;
			}
			else
			{
				closedBranchCount++;
			}
			
			var ch = new List<char>(chars);
			ch.Add(option);
			GenerateParenthesis_Internal(openBracketCount, closedBranchCount, ch);
			if (option == '(')
			{
				openBracketCount--;
			}
			else
			{
				closedBranchCount--;
			}
			
			ch.Remove(option);
		}
	}
}