VAR newdance = false
VAR girls = "Girls"
VAR guys = "Guys"
VAR GirlActor1 = ""
VAR GirlActor2 = ""
VAR GirlActor3 = ""
VAR GirlActor4 = ""
VAR GirlActor5 = ""
VAR GirlActor6 = ""
VAR GuyActor1 = ""
VAR GuyActor2 = ""
VAR GuyActor3 = ""
VAR GuyActor4 = ""
VAR GuyActor5 = ""
VAR GuyActor6 = ""
VAR Player1 = ""
VAR Player2 = ""
VAR Player3 = ""
VAR Player4 = ""

-> HowShouldWeDance

=== HowShouldWeDance ==
I know our dancers have learned a few new moves.
How should we dance this year, let us vote.
* Traditional -> AfterVote
* New dance -> VoteNew -> AfterVote

=== VoteNew ===
~ newdance = true
->->

=== AfterVote ===
it is. {ADDCOMMAND("pause 100")} # BREAK

-> END

=== function EventSetup(girl1,girl2,girl3,girl4,girl5,girl6,guy1,guy2,guy3,guy4,guy5,guy6,farmer1,farmer2,farmer3,farmer4) ===
~ GirlActor1 = girl1
~ GirlActor2 = girl2
~ GirlActor3 = girl3
~ GirlActor4 = girl4
~ GirlActor5 = girl5
~ GirlActor6 = girl6
~ GuyActor1 = guy1
~ GuyActor2 = guy2
~ GuyActor3 = guy3
~ GuyActor4 = guy4
~ GuyActor5 = guy5
~ GuyActor6 = guy6
~ Player1 = farmer1
~ Player2 = farmer2
~ Player3 = farmer3
~ Player4 = farmer4
~ ShowStart()
~ Dance()
~ return "pause 500"

=== function Dance() ===
{not newdance:
    ~ return TraditionalDance()
    - else:
    ~ return NewDance()
}

=== function StartDance() ===
~ Warp(Player1,5,21)
~ Warp(Player2,11,21)
~ Warp(Player3,23,21)
~ Warp(Player4,12,21)
~ FaceAll(2)

=== function SetDanceFraming(music) ===
~ ShowFrameAll(girls,40,0,2)
~ ShowFrameAll(guys,44,12,0)
~ ViewportClamped(14,24)
~ Pause(2000)
~ PlayMusic(music)
~ Pause(600)

=== function TraditionalDance() ===
~ StartDance()
~ Warp(GirlActor1,13,24)
~ Warp(GirlActor2,15,24)
~ Warp(GirlActor3,11,24)
~ Warp(GirlActor4,9,24)
~ Warp(GirlActor5,17,24)
~ Warp(GirlActor6,19,24)
~ Warp(GuyActor1,13,27)
~ Warp(GuyActor2,15,27)
~ Warp(GuyActor3,11,27)
~ Warp(GuyActor4,9,27)
~ Warp(GuyActor5,17,27)
~ Warp(GuyActor6,19,27)
~ SetDanceFraming("FlowerDance")
~ AnimateAll("43 41 43 42","596 4 0", girls, 600)
~ AnimateAll("44 45","12 13 12 14", guys, 600)
~ Pause(9600)
~ AnimateAll("46 47", "596 4 0", girls, 600)
~ AnimateAll("46 47","150 12 13 12 14", guys, 300)
~ RepeatOffsetAllAndPause(15,guys,0,-2,300)
~ RepeatOffsetAllAndPause(12,guys,0,-2,300)
~ OffsetAll(0,-2,guys)
~ AnimateAll("43 41 43 42","596 4 0", girls, 600)
~ AnimateAll("44 45","12 13 12 14", guys, 600)
~ ShowEnd()

=== function NewDance() ===
~ StartDance()
~ Warp(GirlActor1,7,24)
~ Warp(GirlActor2,13,24)
~ Warp(GirlActor3,19,24)
~ Warp(GirlActor4,7,27)
~ Warp(GirlActor5,13,27)
~ Warp(GirlActor6,19,27)
~ Warp(GuyActor1,9,25)
~ Warp(GuyActor2,15,25)
~ Warp(GuyActor3,21,25)
~ Warp(GuyActor4,9,28)
~ Warp(GuyActor5,15,28)
~ Warp(GuyActor6,21,28)
~ SetDanceFraming("honkytonky")
~ Pause(2000)
~ AnimateAll("43 41 43 42","596 4 0", girls, 600)
~ AnimateAll("44 45","12 13 12 14", guys, 600)
~ temp down = 8
~ temp up = down * -1
~ temp left = up
~ temp right = down
~ temp hold = 100
~ RepeatOffsetBothAndPause(8,0,up,0,down,hold)
~ RepeatOffsetBothAndPause(16,left,0,right,0,hold)
~ RepeatOffsetBothAndPause(8,0,down,0,up,hold)
~ RepeatOffsetBothAndPause(16,right,0,left,0,hold)
~ RepeatOffsetBothAndPause(8,0,up,0,down,hold)
~ RepeatOffsetBothAndPause(16,left,0,right,0,hold)
~ RepeatOffsetBothAndPause(8,0,down,0,up,hold)
~ RepeatOffsetBothAndPause(16,right / 2,0,left / 2,0,hold)
~ ShowEnd()

=== function ShowStart() ===
~ PlayMusic("none")
~ Pause(500)
~ GlobalFade()
~ Viewport(-1000,-1000)
~ LoadActors("MainEvent")


=== function ShowEnd() ===
~ Pause(7600)
~ StopAll()
~ Viewport(-1000,-1000)
~ GlobalFade()
~ Message("\"That was fun! Time to go home...\"")
~ EndFestival()
~ CONTINUE()

=== function AnimateAll(animation,alt,who,duration) ===
~ temp commands = ""
{who == girls:
~  commands += GetAnimation(GirlActor1,animation,alt,duration)
~  commands += "/" + GetAnimation(GirlActor2,animation,alt,duration)
~  commands += "/" + GetAnimation(GirlActor3,animation,alt,duration)
~  commands += "/" + GetAnimation(GirlActor4,animation,alt,duration)
~  commands += "/" + GetAnimation(GirlActor5,animation,alt,duration)
~  commands += "/" + GetAnimation(GirlActor6,animation,alt,duration)
- else:
~  commands += GetAnimation(GuyActor1,animation,alt,duration)
~  commands += "/" + GetAnimation(GuyActor2,animation,alt,duration)
~  commands += "/" + GetAnimation(GuyActor3,animation,alt,duration)
~  commands += "/" + GetAnimation(GuyActor4,animation,alt,duration)
~  commands += "/" + GetAnimation(GuyActor5,animation,alt,duration)
~  commands += "/" + GetAnimation(GuyActor6,animation,alt,duration)
}
~ ADDCOMMAND(commands)

=== function GetAnimation(who,animation,alt,duration) ===
{ who == Player1 or who == Player2 or who == Player3 or who == Player4: 
    ~ return "animate " + who + " false true " + duration + " " + alt
    - else:
    ~ return "animate " + who + " false true " + duration + " " + animation
}

=== function Pause(time) ===
~ ADDCOMMAND("pause " + time)

=== function PlayMusic(music) ===
~ ADDCOMMAND("playMusic " + music)

=== function Warp(who, x,y) ===
~ ADDCOMMAND("warp " + who + " " + x + " " + y)

=== function RepeatOffsetAllAndPause(times,who,x,y,time) ===
~ OffsetAll(x,y,who)
~ Pause(time)
~ times--
{times > 0:
    ~ RepeatOffsetAllAndPause(times,who,x,y,time)
    - else:
    ~return
}

=== function RepeatOffsetBothAndPause(times,x,y,x2,y2,time) ===
~ OffsetAll(x,y,guys)
~ OffsetAll(x2,y2,girls)
~ Pause(time)
~ times--
{times > 0:
    ~ RepeatOffsetBothAndPause(times,x,y,x2,y2,time)
    - else:
    ~return
}


=== function OffsetAll(x,y, who) ===
~ temp commands = ""
{who == girls:
~  commands += GetOffset(GirlActor1,x,y)
~  commands += "/" + GetOffset(GirlActor2,x,y)
~  commands += "/" + GetOffset(GirlActor3,x,y)
~  commands += "/" + GetOffset(GirlActor4,x,y)
~  commands += "/" + GetOffset(GirlActor5,x,y)
~  commands += "/" + GetOffset(GirlActor6,x,y)
- else:
~  commands += GetOffset(GuyActor1,x,y)
~  commands += "/" + GetOffset(GuyActor2,x,y)
~  commands += "/" + GetOffset(GuyActor3,x,y)
~  commands += "/" + GetOffset(GuyActor4,x,y)
~  commands += "/" + GetOffset(GuyActor5,x,y)
~  commands += "/" + GetOffset(GuyActor6,x,y)
}
~ ADDCOMMAND(commands)


=== function GetOffset(actor, x, y) ===
~ return "positionOffset " + actor + " " + x + " " + y

=== function FaceAll(direction) ===
~ Face(Player1,direction)
~ Face(Player2,direction)
~ Face(Player3,direction)
~ Face(Player4,direction)

=== function Face(who,direction) ===
~ ADDCOMMAND("faceDirection "+ who + " " + direction)

=== function ShowFrameAll(who,frame,alt,dir) ===
~ temp commands = ""
{who == girls:
~  commands += GetShowFrame(GirlActor1,frame,alt,dir)
~  commands += "/" + GetShowFrame(GirlActor2,frame,alt,dir)
~  commands += "/" + GetShowFrame(GirlActor3,frame,alt,dir)
~  commands += "/" + GetShowFrame(GirlActor4,frame,alt,dir)
~  commands += "/" + GetShowFrame(GirlActor5,frame,alt,dir)
~  commands += "/" + GetShowFrame(GirlActor6,frame,alt,dir)
- else:
~  commands += GetShowFrame(GuyActor1,frame,alt,dir)
~  commands += "/" + GetShowFrame(GuyActor2,frame,alt,dir)
~  commands += "/" + GetShowFrame(GuyActor3,frame,alt,dir)
~  commands += "/" + GetShowFrame(GuyActor4,frame,alt,dir)
~  commands += "/" + GetShowFrame(GuyActor5,frame,alt,dir)
~  commands += "/" + GetShowFrame(GuyActor6,frame,alt,dir)
}
~ ADDCOMMAND(commands)

=== function GetShowFrame(who,frame,alt,dir) ===
{ who == Player1 or who == Player2 or who == Player3 or who == Player4: 
    ~ return "showFrame " + who + " " + alt + "/faceDirection " + who + " " + dir
    - else:
    ~ return "showFrame " + who + " " + frame
}

=== function Message(text) ===
~ ADDCOMMAND("message " + text)

=== function GlobalFade() ===
~ ADDCOMMAND("globalFade")

=== function Viewport(x,y) ===
~ ADDCOMMAND("viewport " + x + " " + y)

=== function ViewportClamped(x,y) ===
~ ADDCOMMAND("viewport " + x + " " + y + " clamp true")

=== function LoadActors(which) ===
~ ADDCOMMAND("loadActors " + which)

=== function StopAll() ===
~ temp commands = ""

~  commands += GetStop(GirlActor1)
~  commands += "/" + GetStop(GirlActor2)
~  commands += "/" + GetStop(GirlActor3)
~  commands += "/" + GetStop(GirlActor4)
~  commands += "/" + GetStop(GirlActor5)
~  commands += "/" + GetStop(GirlActor6)
~  commands += "/" + GetStop(GuyActor1)
~  commands += "/" + GetStop(GuyActor2)
~  commands += "/" + GetStop(GuyActor3)
~  commands += "/" + GetStop(GuyActor4)
~  commands += "/" + GetStop(GuyActor5)
~  commands += "/" + GetStop(GuyActor6)

~ ADDCOMMAND(commands)

=== function GetStop(who) ===
~ return "stopAnimation " + who

=== function EndFestival() ===
~ ADDCOMMAND("waitForOtherPlayers festivalEnd/end")

EXTERNAL ADDCOMMAND(command)
EXTERNAL CONTINUE()

=== function ADDCOMMAND(command) ===
~ return

=== function CONTINUE() ===
~ return