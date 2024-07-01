using System;
using System.Reflection.PortableExecutable;
using System.Threading.Channels;

public enum Subject { math, computers }
public enum DayOfWeek
{
    Pondělí,
    Úterý,
    Středa,
    Čtvrtek,
    Pátek
}

public class Plan
{
    public PlanEvent[] events;

    public Plan(PlanEvent[] events)
    {
        this.events = events;
    }

    public bool IsConflict()
    {
        for(int i = 0; i < events.Length; i++)
        {
            for(int j = i+1; j < events.Length; j++)
            {
                if (this.events[i].IsInConflict(this.events[j]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsOK()
    {
        int countMath = 0;
        int countInf = 0;

        bool[] maths = new bool[events.Length];
        bool[] infs = new bool[events.Length];

        foreach (PlanEvent item in events)
        {
            if(item.subject == Subject.math)
            {
                countMath++;
                if (maths[item.dayOfWeek])
                {
                    return false;
                }
                else
                {
                    maths[item.dayOfWeek] = true;
                }
            }
            if (item.subject == Subject.computers)
            {
                countInf++;
                if (infs[item.dayOfWeek])
                {
                    return false;
                }
                else
                {
                    infs[item.dayOfWeek] = true;
                }
            }
        }

        if (countMath < 3 || countInf < 2)
        {
            return false;
        }

        return true;
    }
}

public class PlanEvent
{
    /* Jmeno tutora */
    public String tutor;
    /* Hodina pocatku doucovani (10 = 10:00 atd.) */
    public int start;
    /* Hodina konce doucovani (10 = 10:00 atd.) */
    public int end;
    /* Den tydne doucovani (0 = Pondeli, 1 = Utery atd.) */
    public int dayOfWeek;
    /* Doucovany predmet */
    public Subject subject;


    static int count = 0;

    /*
	 * Vytvori novou nabidku tutora na doucovani
	 */
    public PlanEvent(string tutor, int start, int end, int dayOfWeek, Subject subject)
    {
        this.tutor = tutor;
        this.start = start;
        this.end = end;
        this.dayOfWeek = dayOfWeek;
        this.subject = subject;
    }

    /*
	 * Vrati true, pokud se tato udalost prekryva se zadanou udalosti, jinak vrati false
	 */
    public bool IsInConflict(PlanEvent other)
    {
        if (this.dayOfWeek != other.dayOfWeek)
        {
            return false;
        }
        if (this.end <= other.start)
        {
            return false;
        }
        if (other.end <= this.start)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// Metoda načte data a vrátí vytvořenou instanci třídy <c>PlanEvent</c> představující jednu nabídku doučování
    /// </summary>
    /// <param name="sr">Reference na <c>StreamReader</c> s otevřeným datovým souborem</param>
    /// <returns>
    /// Nabídka jednoho doučování třídy <c>PlanEvent</c> 
    /// </returns>
    public static PlanEvent LoadData(StreamReader sr)
    {
        PlanEvent e;
        string[] temp = new string[5];

        for (int i = 0; i < 5; i++)
        {
            temp[i] = sr.ReadLine();
        }

        if (temp[1] == "math")
        {
            e = new PlanEvent(temp[0], int.Parse(temp[3]), int.Parse(temp[4]), int.Parse(temp[2]), Subject.math);
        }
        else if (temp[1] == "computers")
        {
            e = new PlanEvent(temp[0], int.Parse(temp[3]), int.Parse(temp[4]), int.Parse(temp[2]), Subject.computers);
        }
        else
        {
            e = null;
        }

        return e;
    }

    /// <summary>
    /// Metoda vypíše na standardní výstup již nekonfliktní zformátovaný rozvrh
    /// </summary>
    /// <param name="plan">Pole obsahující nekonfliktní nabídky doučování</param>
    public static void PrintPlan(PlanEvent[] plan)
    {
        Console.WriteLine($"Plán číslo {PlanEvent.count}");
        Console.WriteLine("------------------------------------------------------------------------");
        Console.WriteLine("{0,-20} | {1,-10} | {2,-10} | {3, -10} | {4, -10}", "Tutor", "Předmět", "Den", "Od hodina", "Do hodina");
        Console.WriteLine("------------------------------------------------------------------------");
        foreach (PlanEvent evnt in plan)
        {
            Console.WriteLine("{0,-20} | {1,-10} | {2,-10} | {3, -10} | {4, -10}", evnt.tutor, evnt.subject, (DayOfWeek)evnt.dayOfWeek, evnt.start, evnt.end);
        }
        Console.WriteLine("------------------------------------------------------------------------");
        Console.WriteLine();
    }

    /// <summary>
    /// Metoda projde všechny možné kombinace nabídek o velikosti <paramref name="set"/> a vybere ty z nich, které nejsou v konfliktu podle metod
    /// <seealso cref="Plan.IsConflict()"/> a <seealso cref="Plan.IsOK()"/> s ostatními v kombinaci  
    /// </summary>
    /// <param name="events">Pole všech nabídek <c>PlanEvent</c></param>
    /// <param name="temp">Pole jedné kombinace nabídek</param>
    /// <param name="start">Počáteční hodnota cyklu</param>
    /// <param name="end">Velikost množiny, ze které se prvky vybírají</param>
    /// <param name="index">Pozice v poli <paramref name="temp"/>, se kterou se právě pracuje</param>
    /// <param name="set">Kolik se vybírá prvků z množiny</param>
    public static void GetOffersCombinations(PlanEvent[] events, PlanEvent[] temp, int start, int end, int index, int set)
    {
        if(index == set)
        {
            Plan plan = new Plan(temp);
            
            if(!plan.IsConflict() && plan.IsOK())
            {
                PlanEvent.count++;
                PrintPlan(temp);
            }
            return;
        }

        for (int i = start; i < end; i++)
        {
            temp[index] = events[i];
            GetOffersCombinations(events, temp, i + 1, end, index+1, set);
        }
    }

    public static void Main(String[] args)
    {
        /*
        PlanEvent event1 = new PlanEvent("František Vonásek", 10, 13, 1, Subject.math);
        PlanEvent event2 = new PlanEvent("Čeněk Landsmann", 9, 12, 1, Subject.computers);
        PlanEvent event3 = new PlanEvent("Hubert Zámožný", 11, 14, 1, Subject.math);
        PlanEvent event4 = new PlanEvent("Dobromila Musilová-Wébrová", 9, 14, 1, Subject.computers);
        PlanEvent event5 = new PlanEvent("Sisoj Psoič Rispoloženskyj", 11, 12, 1, Subject.math);
        PlanEvent event6 = new PlanEvent("Billy Blaze", 8, 10, 1, Subject.computers);
        PlanEvent event7 = new PlanEvent("Flynn Taggart", 13, 15, 1, Subject.math);
        
        
        PlanEvent event1 = new PlanEvent("František Vonásek", 10, 13, 1, Subject.math);
        PlanEvent event2 = new PlanEvent("Čeněk Landsmann", 9, 12, 1, Subject.computers);
        PlanEvent event3 = new PlanEvent("Hubert Zámožný", 11, 14, 2, Subject.math);
        PlanEvent event4 = new PlanEvent("Dobromila Musilová-Wébrová", 9, 14, 2, Subject.computers);
        PlanEvent event5 = new PlanEvent("Sisoj Psoič Rispoloženskyj", 11, 12, 3, Subject.math);
        PlanEvent event6 = new PlanEvent("Billy Blaze", 8, 10, 1, Subject.computers);
        PlanEvent event7 = new PlanEvent("Flynn Taggart", 13, 15, 1, Subject.math);
        
        Console.WriteLine(event1.IsInConflict(event2));
        Console.WriteLine(event1.IsInConflict(event3));
        Console.WriteLine(event1.IsInConflict(event4));
        Console.WriteLine(event1.IsInConflict(event5));
        Console.WriteLine(event1.IsInConflict(event6));
        Console.WriteLine(event1.IsInConflict(event7));

        //PlanEvent[] events = [event1, event2, event3, event4, event5, event6, event7];
        PlanEvent[] events = [event1, event2, event3, event4, event5];

        Plan plan = new Plan(events);
        Console.WriteLine(plan.IsConflict());
        Console.WriteLine(plan.IsOK());
        
        */
        string file = "sscUTF8.txt";
        int numOfLines = File.ReadLines(file).Count();

        FileStream fs = new FileStream(file, FileMode.Open);
        StreamReader sr = new StreamReader(fs);

        PlanEvent[] events = new PlanEvent[numOfLines/5];

        for (int j = 0; j < numOfLines / 5; j++)
        {
            events[j] = LoadData(sr);
        }

        PlanEvent[] eventsComb = new PlanEvent[5];
        int index = 0;
        int start = 0;
        int end = numOfLines/5;

        GetOffersCombinations(events, eventsComb, start, end, index, 5);

        Console.WriteLine("Počet nekonfliktních kombinací nabídek: " + PlanEvent.count);

        sr.Close();
        fs.Close();
    }
}