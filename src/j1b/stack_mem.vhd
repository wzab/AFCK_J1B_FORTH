-- This file is the translation of the stack2.v file written by James Bowman
-- for his J1 Forth CPU.
-- It has been translated to VHDL by Wojciech M. Zabolotny
-- The file is licensed under the original J1 license.
library ieee;
use ieee.std_logic_1164.all;
use ieee.numeric_std.all;
use std.textio.all;

entity stack is
  generic (
    WIDTH : integer := 32;
    DEPTH : integer
    );
  port (
    clk     : in  std_logic;
    clk_ena : in  std_logic;
    rd      : out std_logic_vector(WIDTH-1 downto 0);
    we      : in  std_logic;
    delta   : in  std_logic_vector(1 downto 0);
    wd      : in  std_logic_vector(WIDTH-1 downto 0)
    );
end entity;

architecture rtl of stack is

  type STACK_MEM is array (0 to DEPTH-1) of std_logic_vector(WIDTH-1 downto 0);
  shared variable stack_var : STACK_MEM;

  signal move : std_logic;

  signal headptr                : integer range 0 to DEPTH-1;
  signal head, headN, stack_top : std_logic_vector(WIDTH-1 downto 0);


begin
  move  <= delta(0);
  headN <= wd when we = '1' else stack_top;

  process (clk) is
    variable newheadptr : integer range 0 to DEPTH-1;
  begin  -- process
    if clk'event and clk = '1' then     -- rising clock edge
      if clk_ena = '1' then
        if (we = '1') or (move = '1') then
          head <= headN;
        end if;
        if (move = '1') then
          if delta(1) = '1' then
            if headptr = 0 then
              newheadptr := DEPTH-1;
            else
              newheadptr := headptr-1;
            end if;
            stack_var(headptr) := x"55aa55aa";
            stack_top          <= stack_var(newheadptr);
          else
            if headptr = DEPTH-1 then
              newheadptr := 0;
            else
              newheadptr := headptr + 1;
            end if;
            stack_var(newheadptr) := head;
            stack_top             <= head;
          end if;
          headptr <= newheadptr;
        end if;
      end if;
    end if;
  end process;
  rd <= head;
end architecture;

