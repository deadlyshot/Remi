﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server {
    class HandleGame {

        public static void Formatie(String msg1, String msg2, String msg3, String nickname, String row) {
            int a=Server.pieces[Int32.Parse(msg1)];
            int b=Server.pieces[Int32.Parse(msg2)];
            int c=Server.pieces[Int32.Parse(msg3)];
            Console.WriteLine(a+":"+b+":"+c+":"+nickname+":"+row);
            String res=testeaza(a, b, c, nickname, row);

            if(res.Equals("Nu este o formatie")) {
                MessageSender.MsgtoClient(nickname, "DONT:"+res, Server.clientsInGame);
            } else {
                MessageSender.Broadcast("FORMATION:", nickname, msg1+":"+msg2+":"+msg3, Server.clientsInGame);
            }
            Console.WriteLine(res);
        }

        private static string testeaza(int a, int b, int c, String nickname, String row) {
            if(testInitialNum(a, b)&&testFinalNum(b, c)&&testIntNum(a, c)) {
                Server.formations.Add(numCode(a, b, c, nickname, row));
                return "formatie";
            } else if(testPereche(a, b)&&testPereche(b, c)&&testPereche(a, c)) {
                Server.formations.Add(missingPiece(a, b, c, nickname, row));
                return "formatie";
            } else {
                return "Nu este o formatie";
            }
        }

        private static string numCode(int a, int b, int c, String nickname, String row) {
            if(a==500&&c==500) {
                a=b-1;
                c=b+1;
            } else if(c==500) {
                if(b.ToString().Substring(1, 2).Equals("13"))
                    c=b-12;
                else
                    c=a+2;
            } else if(a==500) {
                a=c-2;
            }
            return "1:"+a+":"+c+":"+nickname+":"+row;
        }

        private static string missingPiece(int a, int b, int c, String nickname, String row) {
            int a1=Int32.Parse(a.ToString().Substring(0, 1));
            int b1=Int32.Parse(b.ToString().Substring(0, 1));
            int c1=Int32.Parse(c.ToString().Substring(0, 1));
            var list=new List<int>(new[] { 1, 2, 3, 4, 5 });
            var result=list.Except(new[] { a1, b1, c1 });
            int color1=result.ElementAt(0);
            int color2=result.ElementAt(1);
            String number=a.ToString().Substring(1, 2);
            if(number=="00") {
                number=b.ToString().Substring(1, 2);
            }
            if(number=="00") {
                number=c.ToString().Substring(1, 2);
            }
            return "2:"+color1+number+":"+color2+number+":"+nickname+":"+row;
        }
        public static bool testInitialNum(int a, int b) {
            if(a==500&&( b==101||b==201||b==301||b==401 )) {
                return false;
            } else if(( b-a )==1||a==500||b==500) {
                return true;
            } else {
                return false;
            }
        }
        public static bool testFinalNum(int a, int b) {
            if(( b-a )==1||( a-b )==12||a==500||b==500) {
                return true;
            } else {
                return false;
            }
        }
        public static bool testIntNum(int a, int b) {// 11 12 13
            if(( b-a )==2||a==500||b==500||( a-b )==11) {
                return true;
            } else {
                return false;
            }
        }
        public static bool testPereche(int a, int b) {
            if(Math.Abs(a-b)==100||Math.Abs(a-b)==200||Math.Abs(a-b)==300||a==500||b==500) {
                return true;
            } else {
                return false;
            }
        }

        internal static void AddPiece(string row, string imageIndex, string column, string clientToAdd, string nickname) {
            String formationCod=Server.formations.Cast<String>().First(e => e.Split(':').ElementAt(3).Equals(clientToAdd)&&
                             e.Split(':').ElementAt(4).Equals(row));
            string[] s=formationCod.Split(':');//1:404:406:nickname:row
            string formationType=s[0];
            int firstPiece=Int32.Parse(s[1]);
            int pieceAfterFirst=firstPiece+1;
            int lastPiece=Int32.Parse(s[2]);
            int pieceBeforeLast=lastPiece-1;
            int pieceToAdd=Server.pieces[Int32.Parse(imageIndex)];
            if(formationType.Equals("1")) {//numaratoare
                if(testInitialNum(pieceToAdd, firstPiece)//pieceToAdd:firstPiece:pieceAfterFirst
                    &&testIntNum(pieceToAdd, pieceAfterFirst)
                    &&column.Equals("0")) {
                    MessageSender.Broadcast("ADD_PIECE_ON_FIRST_COL:", clientToAdd, row+":"+imageIndex+":"+column, Server.clientsInGame);
                    int index=Array.IndexOf(Server.formations.ToArray(), formationCod);
                    Server.formations[index]=formationType+":"+pieceToAdd+":"+lastPiece+":"+clientToAdd+":"+row;
                } else if(testFinalNum(lastPiece, pieceToAdd)//pieceBeforeLast:lastPiece:pieceToAdd
                    &&testIntNum(pieceBeforeLast, pieceToAdd)
                    &&!( pieceBeforeLast==100||pieceBeforeLast==200||pieceBeforeLast==300||pieceBeforeLast==400 )
                    &&!column.Equals("0")) {
                    MessageSender.Broadcast("ADD_PIECE:", clientToAdd, row+":"+imageIndex+":"+column, Server.clientsInGame);
                    int index=Array.IndexOf(Server.formations.ToArray(), formationCod);
                    Server.formations[index]=formationType+":"+firstPiece+":"+pieceToAdd+":"+clientToAdd+":"+row;
                } else {
                    MessageSender.MsgtoClient(nickname, "DONT:Nu se lipeste!", Server.clientsInGame);
                }
            } else if(formationType.Equals("2")) {//pereche
                if(pieceToAdd==firstPiece||pieceToAdd==lastPiece) {
                    MessageSender.Broadcast("ADD_PIECE:", clientToAdd, row+":"+imageIndex+":"+column, Server.clientsInGame);
                    int index=Array.IndexOf(Server.formations.ToArray(), formationCod);
                    Server.formations[index]=formationType+":0:0:"+clientToAdd+":"+row;
                } else {
                    MessageSender.MsgtoClient(nickname, "DONT:Nu se lipeste!", Server.clientsInGame);
                }
            }
        }
    }
}