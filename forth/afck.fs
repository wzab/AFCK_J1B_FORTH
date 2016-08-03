\ Forth procedures for configuration of internal chips on the AFCK board
\ Written by Wojciech M. Zabolotny
\ ( wzab01<at>gmail.com or wzab<at>ise.pw.edu.pl )
\ It is available as PUBLIC DOMAIN or under Creative Commons CC0 License
\
hex
0201 constant I2C_BUS_SEL 
decimal

\ Procedures for selecting the bus handled by the J1B I2C controller
: bus_sel ( n_bus )
    I2C_BUS_SEL io!
;
decimal
\ I2C_MUX
112 constant I2C_MUX 
10 constant I2C_MUX_Si57x \ Value required to access Si57x
12 constant I2C_MUX_ADN4604 \ Value required to access ADN4604

\ Indexed access (to ADN4604 and Si57x)
create I2C_2buf 2 c, 0 c, 0 c, \ Buffer of length 2
: I2C_ind_rd ( reg_nr addr -- val )
    dup ( reg_nr addr addr )
    -rot ( addr reg_nr addr )
    i2c_wr1
    i2c_rd1
;    

: I2C_ind_wr ( reg_nr val addr )
    -rot
    I2C_2buf 1+ ( addr reg_nr val I2C_2buf+1 )
    swap ( addr reg_nr I2C_2buf+1 val )
    over 1+ ( addr reg_nr I2C_2buf+1 val I2C_2buf+2 )
    c! c! ( addr )
    I2C_2buf swap i2c_wr    
;

\ ADN4604 routines
75 constant ADN4604 


: ADN4604_rd ( reg_nr -- val )
    ADN4604 I2C_ind_rd
;

: ADN4604_wr ( reg_nr val -- )
    ADN4604 I2C_ind_wr
;

\ Si57x routines
85 constant Si57x 
variable Si57x_old_mux \ It can't be used in recursive procedures!

: Si57x_wr ( addr val -- )
    Si57x I2C_ind_wr
;

: Si57x_rd ( addr -- val )
    Si57x I2C_ind_rd
;

\ The procedure below sets the frequency
2variable S5_RFREQ
2variable S5_FDCO
variable S5_N1
variable S5_HSDIV
variable S5_FXTAL
\ Constants needed to calculate the crystal frequency
decimal 100000000 hex 10000000 decimal um* 2constant 100E6*1<<28
decimal 4850 1000000 um* 2constant S5_FDCOL
decimal 5670 1000000 um* 2constant S5_FDCOH

decimal
create s5_hsdvs  11  , 9 ,  7 , 6 , 5 , 4 ,
\ The implementation is split between a few words for easier testing

hex \ It is easier to define bit manipulation constants in hex mode
: Si57x_read_setgs
    \ Reset Si57x to initial settings
    87 01 Si57x_wr
    \ read RFREQ and HSDIV
    7 Si57x_rd ( frq r7 )
    dup 5 rshift 4 + S5_HSDIV !
    1f and 2 lshift ( frq N1[6:2] )
    8 Si57x_rd ( frq N1[6:2] r8)
    dup 6 rshift ( frq N1[6:2] r8 N1[1:0] )
    rot or ( frq r8 N1 )
    1+ \ We increase N1 by 1 according to docs
    S5_N1 ! ( frq r8 )
    \ In the procedure below we utilize the variable storage model used by swapforth
    \ Each word is stored in LE order, but the more significant word in double precision variable
    \ is stored first!
    \ To summarize: byte0-bits 39:32, b1-47:40, b2-55:48, b3-63:56, b4-7:0, b5-15:8, b6-23:16, b7-31:24
    0. S5_RFREQ 2! ( frq r8) 
    3f and S5_RFREQ c! \ Store bits 39(37):32
    9 Si57x_rd ( frq r9 )
    S5_RFREQ 7 + c! \ Bits 31:24
    a Si57x_rd ( frq r10 )
    S5_RFREQ 6 + c! \ Bits 23:16
    b Si57x_rd ( frq r11 )
    S5_RFREQ 5 + c! \ Bits 15:8
    c Si57x_rd ( frq r12 )
    S5_RFREQ 4 + c! \ Bits 7:0
;

decimal
\ Simulate reading for test purposes
: Si57x_sim_read_setgs
    7528285000. S5_RFREQ 2!
    4 S5_HSDIV !
    8 S5_N1 !
;

\ Print Si57x settings for debugging purposes
: Si57x_show_setgs
    ." HS_DIV=" S5_HSDIV @ . CR
    ." N1=" S5_N1 @ . CR
    ." FXTAL=" S5_FXTAL @ . CR
    ." RFREQ=" S5_RFREQ 2@ d. CR
    ." FDCO=" S5_FDCO 2@ d. CR
    S5_RFREQ 2@ S5_FXTAL @ S5_HSDIV @ S5_N1 @ * M*/
    \ We must divide it yet by 1 << 28
    1 28 lshift UM/MOD
    ." FOUT=" . drop CR 
    
;


\ The word below calculates settings needed to obtain the desired frequency
\ Please note, that it should be called after Si57x_read_setgs,
\ as the procedure destroys the values produced by Si57x_read_setgs.
decimal
: Si57x_calc_setgs ( frq -- )
    \ First we should calculate fxtal as (100e6 << 28)/rfreq*hsdiv*n1
    \ First calculate hsdiv*n1
    S5_HSDIV @ S5_N1 @ um* ( frq n1*hs_div . )
    \ .s cr
    \ Multiply it by 100E6*1<<28, result will be stored in UDres
    100E6*1<<28 
    \ .s cr
    ud*
    .UDres .UDsub
    \ Divide it by RFREQ and store to S5_FXTAL
    S5_RFREQ 2@ ud/
    \ .s cr
    \ The frequency should fit into single precision
    if
	132 throw
    then
    S5_FXTAL ! ( frq )

    \ Now we should scan possible N1 and HSDIV vals, finding the best matched settings
    6 0 do
	s5_hsdvs i cells + @ S5_HSDIV !
	1 ( frq n1v )
	1 0 do \ This will be indefinite loop ended by 0 +LOOP. That allows creating of loops with multiple exit points
	    ( frq n1v )
	    \ dup .
	    dup 128 > if
		leave
	    then
	    \ Calculate the FDCO value
	    S5_HSDIV @ ( frq n1v hsdv[i] )
	    over * ( frq n1v hsdv[i]*n1v )
	    2 pick um* ( frq n1v fdco . )
	    2dup S5_FDCO 2!
	    \ 2dup d. cr
	    2dup S5_FDCOL D< if
		2drop 
	    else
		S5_FDCOH  D< if
		    \ ." Found! "
		    leave
		then
	    then
	    \ Update N1V
	    dup 2 < if
		1+
	    else
		2 +
	    then	    
	0 +loop
	( frq n1v )
	\ If N1V is below 129, it means, that the correct value was found
	dup 129 < if
	    leave
	then
	drop
    loop ( frq n1v)
    128 over < if
	\ N1V above 128 means, that no correct setting was found
	131 throw
    then
    S5_N1 !
    drop \ drop the frq
    \ Calculate the new value of RFREQ - the NFREQ
    \ 28 is 1c in hex!
    S5_FDCO 2@ 1 28 lshift S5_FXTAL @ M*/ S5_RFREQ 2!    
;    

hex
: Si57x_write_setgs
    89 10 Si57x_wr
    87 30 Si57x_wr
    S5_N1 @ 1-  2 rshift
    S5_HSDIV @ 4 - 5 lshift or
    7 swap Si57x_wr    ( n1-1 )
    S5_RFREQ 4 + c@ \ Bits 7:0
    c swap Si57x_wr ( frq r12 )
    S5_RFREQ 5 + c@ \ Bits 15:8
    b swap Si57x_wr ( frq r11 )
    S5_RFREQ 6 + c@ \ Bits 23:16
    a swap Si57x_wr ( frq r10 )
    S5_RFREQ 7 + c@ \ Bits 31:24
    9 swap Si57x_wr ( frq r9 )
    S5_N1 @ 1- 6 lshift
    S5_RFREQ c@ 3f and or 8 swap Si57x_wr
    89 0 Si57x_wr
    87 40 Si57x_wr
;    

: Si57x_set_frq ( frq -- )
    \ Save old mux and set mux to access Si57x
    I2C_MUX i2c_rd1 Si57x_old_mux !
    I2C_MUX_Si57x I2C_MUX i2c_wr1
    Si57x_read_setgs
    Si57x_calc_setgs
    Si57x_write_setgs
    Si57x_old_mux @ I2C_MUX i2c_wr1
;    
decimal

\ Procedures to control the clock matrix
hex
: ClkMtx_set_out ( n_in n_out -- )
    over -1 < ( n_in n_out flag )
    2 pick f > or if
	." n_in must be between -1 and 15 "
	83 throw
    then ( n_in n_out )
    dup 0 < over f > or if
	." n_out must be between 0 and 15 "
	83 throw
    then ( n_in n_out )
    4 bus_sel
    I2C_MUX i2c_rd1 Si57x_old_mux !
    I2C_MUX_ADN4604 I2C_MUX i2c_wr1
    over -1 = if 
	\ Switch off the output
	20 + 0 ADN4604_wr drop 
    else
	\ Select the input and switch the output
	\ Number of register as 0x90+n_out/2
	\ Number of nibble as n_out % 2
	\ Switch according to the nibble
	dup 1 and if
	    swap 4 lshift
	    0f ( n_out n_in*16 mask )
	else
	    swap
	    f0 ( n_out n_in mask )
	then ( n_out n_in_shifted mask )
	\ Calculate the number of register
	rot ( n_in_shifted mask n_out )
	dup >r  ( n_in_shifted mask n_out ) ( R: n_out)
	1 rshift 90 + >r ( n_in_shifted mask ) ( R: n_out reg_adr )
	r@ ADN4604_rd ( n_in_shifted mask old_val)
	and or ( new_val ) ( R: n_out reg_adr )
	r> swap ADN4604_wr
	\ Trigger matrix update
	81 00 ADN4604_wr
	80 01 ADN4604_wr
	\ Switch on the output
	r> 20 + 30 ADN4604_wr
    then
    Si57x_old_mux @ I2C_MUX i2c_wr1
;
decimal
