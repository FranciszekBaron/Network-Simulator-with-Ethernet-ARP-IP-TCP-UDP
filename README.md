nauczyłem się kiedy robic magic numbers czasami, kiedy opytmalizacja jest wazna 

czasami moze lepiej nie robic MAGIC NUMBERS na rzecz wiekszej alokacji na stacku 


Użyłem const bo to compile-time constants - zero runtime overhead



TRADEOFS -> wybory co lepssze 
pamiec vs szybkosc 

bepieczenstwo vs wygoda 

czytelnosc vs wydajnosc (nasz przyklad)

god object - trudno utrzymac jeden wielki obiekt, który ma 
duzo za duzo zmiennych -> tutaj czescie SOC




OVERHEADs - dodatkowy koszt potrzebny do zrobienia czegos

wziecie Dictionary, zamiast int[], zajmuje wiecej pamieci na same wartosci + hashtable + pointers

wziecie class(reference type) zamiast struct(value type)


Overhead jest akceptowalny wtedy, kiedy zyskujesz więcej ni tracisz 

"Przedwczesna optymalizacja to źródło wszelkiego zła" - Donald Knuth




Pakiet IP, moze przez zakłocenia sieci się róznić, wtedy
CHEKCSUM ci to powie, ale co sie dzieje? 
Nic!! Nic się nie dzieje, pakiet nie przeslany i tyle

z pomoca przychodzi TCP (patrz ponizej)

UDP - gdy musisz miec szybko, TCP - gdy musisz miec wszystkie dane 


One's Addition Complete - dodaje do momentu przekroczenia maksymalnej wartosci ushort, pozniej bierze przekroczenie i daje to jako sume
To daje zgodnosc operacji, nie 'faktyczna' matematyczna sumę 

Negacja - jest po to ze jak potem sprawdzamy czy checksum sie zgadza to checksum = ~sum , a reszta rowna sie sum wiec jak dodamy to wynik = 0xFFFF



static abstract -> C# v11.0 !!!!!