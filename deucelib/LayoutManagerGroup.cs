namespace deuce;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


public class LayoutManagerGroup : LayoutManagerDefault
{
    public LayoutManagerGroup(float pageWidth, float pageHeight, float pageTopMargin,
        float pageLeftMargin, float pageRightMargin, float pageBottomMargin, float tablePaddingTop, float tablePaddingBottom,
        float tablePaddingLeft = 5f, float tablePaddingRight = 5f)
        : base(pageWidth, pageHeight, pageTopMargin, pageLeftMargin, pageRightMargin, pageBottomMargin, tablePaddingTop, tablePaddingBottom, tablePaddingLeft, tablePaddingRight)
    {
    }

    public override object ArrangeLayout(Tournament tournament)
    {
        List<PagenationInfo> layout = new List<PagenationInfo>();
        int pageIndex = 1;

        // Iterate through each group in the tournament
        foreach (Group group in tournament.Groups)
        {
            if (group.Draw == null) continue; // Skip groups without draws

            // Layout main rounds for this group
            int totalMatches = group.Draw.Rounds.FirstOrDefault(e => e.Index == 1)?.Permutations.Sum(e => e.Matches.Count) ?? 0;
            
            if (totalMatches > 0)
            {
                int totalCols = (int)Math.Log2(totalMatches) + 1;
                int blocsOfCols = (int)Math.Ceiling((double)totalCols / _maxCols);
                
                Console.WriteLine($"Group {group.Label}: Total Matches: {totalMatches}, Total Columns: {totalCols}, Blocks of Columns: {blocsOfCols}");

                // Main round pages for this group
                for (int i = 0; i < blocsOfCols; i++)
                {
                    int startRound = i * _maxCols;
                    int endRound = Math.Min(startRound + _maxCols - 1, totalCols - 1);
                    int matchesInRound = (int)(totalMatches / Math.Pow(2, startRound));
                    int totalPages = (int)Math.Ceiling((double)matchesInRound / _maxRows);
                    int matchesPerPage = Math.Min(matchesInRound, _maxRows);

                    for (int j = 0; j < totalPages; j++)
                    {
                        ArrangePageLayout(layout, startRound, endRound, pageIndex, matchesPerPage, totalMatches, j, false, false, group);
                        pageIndex++;
                    }
                }

                // Layout playoff rounds for this group (if any)
                int playoffMatches = group.Draw.Rounds.FirstOrDefault(e => e.Index == 1)?.Playoff?.Permutations.Sum(e => e.Matches.Count) ?? 0;
                
                if (playoffMatches > 0)
                {
                    totalCols = (int)Math.Log2(playoffMatches) + 1;
                    blocsOfCols = (int)Math.Ceiling((double)totalCols / _maxCols);

                    for (int i = 0; i < blocsOfCols; i++)
                    {
                        int startRound = i * _maxCols;
                        int endRound = Math.Min(startRound + _maxCols - 1, totalCols);
                        int matchesInStartRound = (int)(playoffMatches / Math.Pow(2, startRound));
                        int totalPages = (int)Math.Ceiling((double)matchesInStartRound / _maxRows);
                        int matchesPerPage = Math.Min(matchesInStartRound, _maxRows);

                        for (int j = 0; j < totalPages; j++)
                        {
                            ArrangePageLayout(layout, startRound, endRound, pageIndex, matchesPerPage, playoffMatches, j, true, false, group);
                            pageIndex++;
                        }
                    }
                }

                // Final match for this group
                int lastRoundIdx = (group.Draw.Rounds.Count()) - 1;
                ArrangePageLayout(layout, lastRoundIdx, lastRoundIdx, pageIndex, 1, 1, 0, false, true, group);
                pageIndex++;
            }
        }

        return layout;
    }

    private void ArrangePageLayout(List<PagenationInfo> layout, int startRound, int endRound, int pageIndex,
     int rowsInFirstColumn, int totalMatches, int pageInRound, bool isPlayoffRound = false, bool isFinalMatch = false, Group? group = null)
    {
        RectangleF drawArea = new RectangleF(_pageLeftMargin, _pageTopMargin,
            _pageWidth - _pageLeftMargin - _pageRightMargin,
            _pageHeight - _pageTopMargin - _pageBottomMargin);

        float headerHeight = 30f;
        float headerSpacing = 5f;
        float contentStartY = _pageTopMargin + headerHeight + headerSpacing;
        RectangleF contentArea = new RectangleF(_pageLeftMargin, contentStartY,
            _pageWidth - _pageLeftMargin - _pageRightMargin,
            _pageHeight - contentStartY - _pageBottomMargin);

        float recHeight = (contentArea.Height - _maxRows * (_tablePaddingTop + _tablePaddingBottom)) / _maxRows;
        float recWidth = (drawArea.Width - _maxCols * (_tablePaddingLeft + _tablePaddingRight)) / _maxCols;

        float labelWidth = 120f;
        float labelHeight = headerHeight;
        string labelText = group != null ? $"Group {group.Label}" : "";
        RectangleF labelRect = new RectangleF(
            (_pageWidth - labelWidth) / 2f,
            _pageTopMargin,
            labelWidth,
            labelHeight);
        layout.Add(new PagenationInfo(0, 0, 0, labelRect, labelText, pageIndex, PageElementType.RoundLabel, isPlayoffRound) { IsFinalMatch = isFinalMatch, Group = group });
        for (int r = 0; r <= (endRound - startRound); r++)
        {
            int currentRound = startRound + r;
            int matchesInRound = totalMatches / (int)Math.Pow(2, r);
            int noPlayers = matchesInRound * 2;
            string subSectionText = isPlayoffRound ? "Playoff" : "Main";
            string groupPrefix = group != null ? $"Group {group.Label} - " : "";
            string headerText = isFinalMatch ? $"{groupPrefix}Final" : $"{groupPrefix}Round {currentRound + 1} ({subSectionText})";

            RectangleF headerRect = new RectangleF(
                _pageLeftMargin + r * (recWidth + _tablePaddingLeft),
                _pageTopMargin,
                recWidth,
                headerHeight);

            layout.Add(new PagenationInfo(0, 0, currentRound, headerRect, headerText, pageIndex, PageElementType.RoundHeader, isPlayoffRound) { IsFinalMatch = isFinalMatch, Group = group });
        }

        float totalFirstRowHeight = rowsInFirstColumn * (recHeight + _tablePaddingTop + _tablePaddingBottom);
        float startY = contentStartY + (contentArea.Height - totalFirstRowHeight) / 2f;

        for (int i = 0; i < rowsInFirstColumn; i++)
        {
            RectangleF rect = new RectangleF(_pageLeftMargin,
                                             startY + i * (recHeight + _tablePaddingTop + _tablePaddingBottom),
                                              recWidth,
                                              recHeight);
            int rowOffset = pageInRound * _maxRows + i;
            layout.Add(new PagenationInfo(0, 0, startRound + 1, rect, rowOffset, pageIndex, isPlayoffRound) { IsFinalMatch = isFinalMatch, Group = group });
        }

        for (int r = 1; r <= (endRound - startRound); r++)
        {
            int rows = rowsInFirstColumn / (int)Math.Pow(2, r);
            int prevRound = startRound + r;

            var prevSteps = layout.FindAll(x => x.Round == prevRound && x.ElementType != PageElementType.RoundHeader
            && x.IsPlayoffRound == isPlayoffRound);
            for (int j = 0; j < rows; j++)
            {
                int idx1 = j * 2;
                int idx2 = idx1 + 1;
                if (prevSteps != null && idx2 < prevSteps.Count)
                {
                    var prevRect1 = prevSteps[idx1].Rectangle;
                    var prevRect2 = prevSteps[idx2].Rectangle;
                    float mid1 = prevRect1.Top + prevRect1.Height / 2f;
                    float mid2 = prevRect2.Top + prevRect2.Height / 2f;
                    float center = (mid1 + mid2) / 2f;
                    RectangleF rect = new RectangleF(
                         _pageLeftMargin + r * (recWidth + _tablePaddingLeft),
                        center - recHeight / 2f,
                        recWidth,
                        recHeight);
                    int matchesPerPage = (int)Math.Ceiling((float)rowsInFirstColumn / (float)Math.Pow(2, r));
                    matchesPerPage = Math.Min(matchesPerPage, _maxRows);
                    int rowOffset = pageInRound * matchesPerPage + j;
                    layout.Add(new PagenationInfo(0, 0, startRound + r + 1, rect, rowOffset,
                        pageIndex, isPlayoffRound)
                    { IsFinalMatch = isFinalMatch, Group = group });
                }
            }
        }


    }

    public static string DebugHeaders(List<PagenationInfo> layout)
    {
        var headers = layout.Where(p => p.ElementType == PageElementType.RoundHeader).ToList();
        var labels = layout.Where(p => p.ElementType == PageElementType.RoundLabel).ToList();
        var result = $"Found {headers.Count} round headers and {labels.Count} round labels:\n";

        foreach (var header in headers)
        {
            string groupInfo = header.Group != null ? $" (Group {header.Group.Label})" : "";
            result += $"Round Header {header.Round}: '{header.Text}'{groupInfo} at ({header.Rectangle.X}, {header.Rectangle.Y})\n";
        }

        foreach (var label in labels)
        {
            string groupInfo = label.Group != null ? $" (Group {label.Group.Label})" : "";
            result += $"Round Label on Page {label.PageIndex}: '{label.Text}'{groupInfo} at ({label.Rectangle.X}, {label.Rectangle.Y})\n";
        }

        return result;
    }
}