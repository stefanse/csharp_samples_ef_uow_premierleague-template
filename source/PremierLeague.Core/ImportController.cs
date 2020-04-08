using System;
using System.Collections.Generic;
using System.Linq;
using PremierLeague.Core.Entities;
using Utils;
using System.IO;

namespace PremierLeague.Core
{
    public static class ImportController
    {
        public static LinkedList<Game> listWithGames = new LinkedList<Game>();
        public static Dictionary<string, Team> teams = new Dictionary<string, Team>();
        public static IEnumerable<Game> ReadFromCsv()
        {
            string fileNameWithFilePath = MyFile.GetFullNameInApplicationTree("PremierLeague.csv");
            string[] currentGameLine;
            string[] gamesAsStringLines;

            try
            {
            gamesAsStringLines = File.ReadAllLines(fileNameWithFilePath, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {

                throw new Exception("An Error has occured",e);
            }

            Game currentGame;
            for (int i = 0; i < gamesAsStringLines.Length; i++)
            {
                currentGameLine = gamesAsStringLines[i].Split(';');
                currentGame = new Game
                {
                    Round = Convert.ToInt32(currentGameLine[0]),
                    HomeTeam = checkIfTeamIsAlreadyinTeams(currentGameLine[1]),
                    GuestTeam = checkIfTeamIsAlreadyinTeams(currentGameLine[2]),
                    HomeGoals = Convert.ToInt32(currentGameLine[3]),
                    GuestGoals = Convert.ToInt32(currentGameLine[4]),
                };
              
                listWithGames.AddLast(currentGame);
            }
          
            return listWithGames;
        }

        public static Team checkIfTeamIsAlreadyinTeams(string teamname)
        {
            if (!teams.ContainsKey(teamname))
            {
                Team newTeam = new Team { Name = teamname };
                teams.Add(teamname, newTeam);
                return newTeam;
            }
            else
            {
                return teams[teamname];
            }
        }
    }
}