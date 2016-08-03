#!/usr/bin/python
fin=open("nuc.hex","r")
dta=fin.readlines()
n=len(dta);
print n
fout=open("prog.vhd","w")
fout.write("""
library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;

library work;

package ram_prog is

type T_RAM_PROG is array(0 to 8191) of std_logic_vector(31 downto 0);
constant ram_init : T_RAM_PROG := (
""")
for i in range(0,n):
  fout.write("  "+str(i)+" => std_logic_vector'(x\""+dta[i].strip()+"\")")
  if i==n-1:
    fout.write(");\n")
  else:
    fout.write(",\n")
fout.write("end package ram_prog;\n")



