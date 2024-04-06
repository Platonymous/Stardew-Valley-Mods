VAR none = ""
VAR emilygift = ""
VAR emilyadvice = ""
VAR alexgift = ""
VAR alexadvice = ""
VAR alexsamegift = false
VAR emilyseen = false
VAR alexseen = false
VAR whenbirthday = "tomorrow"
VAR today = 0

~ emilygift = pickgift(none,none)
~ emilyadvice = pickgift(emilygift,none)
~ alexgift = pickgift(emilygift, emilyadvice)
~ alexadvice = pickgift(emilygift,alexgift)

$h Hi @, {whenbirthday} is my sisters birthday. And I have the perfect gift. # BR
No I can not tell you what it is, it is a surprise.
But...
* [I still need a gift and have no idea] -> noidea
* [I want to make sure we do not have the same gift.] -> samegift

=== noidea ===
Ok, I help you. -> gift

=== samegift ===
That is a good point. -> gift

=== gift ===
$h My gift is {emilygift} she loves, so you should give her something else, maybe {emilyadvice}.
But you should also ask Alex, I know he has a gift for Haley. {setseen()} # BREAK # ADD Alex THIS 
-> alex

=== alex ===
You want to know what gift I have for Haley? Why do you want to know?
* [So we do not have the same gift.] -> whatgift
* [To give her a better one than you.] -> bettergift

=== bettergift ===
 $a {ChangeFS("HA",-10)}, as if. 
 I have the best one. -> whatgift

=== whatgift ===
What did you get her?
* A flower[],  -> sameasalex("a flower") -> flower
* A cake[],  -> sameasalex("a cake") -> cake
* A salad[],  -> sameasalex("a salad") -> salad
* A fruit[],  -> sameasalex("a fruit") -> fruit


=== sameasalex(choice) ===
{choice == alexgift:
    ~ alexsamegift = true
    <>$a {ChangeFS("WHAT?",-10)} That is what I have too. And
    - else:
    <>$k that sound {ChangeFS("good",10)}, and 
}
- ->->

=== flower ===
<> I bet it is the big yellow one too, she loves thoses.
-> endalex

=== salad ===
<> I bet it is the fruity one too, she loves those.
-> endalex

=== fruit ===
<> I bet it is the big brown one too, she loves those.
-> endalex

=== cake ===
<> I bet it is the pink one too, she loves those.
-> endalex


=== endalex ===
{ alexsamegift: 
Get something else like {alexadvice}. I had the idea first.  
- else:
But we do not have the same gifts so it is fine. 
}
<> See you around. {setseen()} # BREAK

	
-> END

=== function ChangeFS(word, change) ===
~ FRIENDSHIP(SPEAKER(),change)
~ return word

=== function ShouldShow(npcname,path,day,season,year) ===
{
    - emilyseen && npcname == "Emily":
        ~ return false
    - day == 14 && npcname == "Emily":
        ~ today++
        ~ whenbirthday = "today"
        ~ return true
    - else
        ~ return true
}

=== function Fallback() ===
~ return "Hi @ Do you have a gift for Haley yet?"

=== function DayEnding(day,season,year) ===
~ today++
~ whenbirthday = "today"
{
    - day >= 14:
        ~ return true
    - emilyseen && alexseen:
        ~ return true
    - today == 1 && not alexseen:
        ~ ADDNEXT("Alex","THIS")
        ~ return false
    - not emilyseen:
        ~ return false
    - day == 13:
        ~ return false
    - else:
        ~ return true
}

=== function setseen() ===
{emilyseen:
    ~ alexseen = true
    - else:
    ~ emilyseen = true
}

=== function pickgift(exclude1,exclude2) ===
    ~ temp g =  "{~a flower|a cake|a salad|a fruit}"
    { g == exclude1 or g == exclude2:
	~ g = pickgift(exclude1, exclude2)
    }
	~ return g
	
EXTERNAL ADD(npc,ink)
EXTERNAL ADDNEXT(npc,ink)
EXTERNAL RESET()
EXTERNAL LOG(text,type)
EXTERNAL SDVCHARS(text,npcname)
EXTERNAL HASMOD(id)
EXTERNAL SPEAKER()
EXTERNAL FRIENDSHIP(npcname, change)
EXTERNAL CHECK(conditions)
EXTERNAL COMMAND(command)
EXTERNAL ADDCOMMAND(command)
EXTERNAL SIN(num)
EXTERNAL COS(num)
EXTERNAL TAN(num)
EXTERNAL STEXT(id,key)
EXTERNAL SNUM(id,key)
EXTERNAL SCOUNT(key,change,isFixed)
EXTERNAL SETSTEXT(key,value,isFixed)
EXTERNAL SETSNUM(key,num,isFixed)

=== function ADDCOMMAND(command) ===
~ return

=== function SIN(num) ===
~ return 0

=== function COS(num) ===
~ return 0

=== function TAN(num) ===
~ return 0

=== function STEXT(id,key) ===
~ return key

=== function SNUM(id,key) ===
~ return 0

=== function SCOUNT(key,change,isFixed) ===
~ return change

=== function SETSTEXT(key,value,isFixed) ===
~ return

=== function SETSNUM(key,num,isFixed) ===
~ return

=== function ADD(npc,ink) ===
~ return

=== function ADDNEXT(npc,ink) ===
~ return

=== function RESET() ===
~ return

=== function LOG(text,type) ===
~ return

=== function SDVCHARS(text,npcname) ===
~ return text

=== function HASMOD ===
~ return true

=== function SPEAKER ===
~ return "Emily"

=== function FRIENDSHIP(npcname, change) ===
~ return 250

=== function CHECK ===
~ return true

=== function COMMAND ===
~ return