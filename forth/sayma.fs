\ Forth procedures for configuration of internal chips on the AFCK board
\ Written by Wojciech M. Zabolotny
\ ( wzab01<at>gmail.com or wzab<at>ise.pw.edu.pl )
\ Extended by Grzegorz H. Kasprowicz ( kasprowg<at>gmail.com )
\ for Sayma board
\ It is available as PUBLIC DOMAIN or under Creative Commons CC0 License
\

decimal
\ Frequency counters 
$0100 constant FRQ0_CNT
$0101 constant FRQ1_CNT
$0102 constant FRQ2_CNT

\ Output registers
$0180 constant OUT0_REG
$0180 constant OUT0_SET_REG
$0184 constant OUT0_CLR_REG
$0181 constant OUT1_REG
$0182 constant OUT2_REG
$0183 constant OUT3_REG

\ Input pins
$0190 constant INP0_REG
$0191 constant INP1_REG
$0192 constant INP2_REG
$0193 constant INP3_REG
$0201 constant I2C_BUS_SEL 




: spi_hmc830 ( a b c )
	rot 31 lshift rot 25 lshift rot 1 lshift or or
	1 OUT0_SET_REG io! \ SCK hi
	1 2 lshift OUT0_SET_REG io! \ deactivate hmc7043
	1 16 lshift OUT0_SET_REG io! \ OE buff active
	1 8 lshift OUT0_SET_REG io! \ SEN high
    32 0 do
        dup
        1 31 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
		1 OUT0_CLR_REG io! \ SCK low
		2*
		INP0_REG io@ 4 rshift 1 and +
        1 OUT0_SET_REG io! \ SCK hi	
    loop
	 1 8 lshift OUT0_CLR_REG io! \ SEN low
	 $ffffff and  \ return only valid bits
;


: spi_adc9154_1 ( a b c )
	rot 23 lshift rot 8 lshift rot 0 lshift or or
	1 5 lshift OUT0_CLR_REG io! \ CS low
    24 0 do
        dup
        1 23 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@ 1 rshift 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	 1 5 lshift OUT0_SET_REG io! \ CS high
	 $ff and  \ return only valid bits
;

: spi_adc9154_2 ( a b c )
	rot 23 lshift rot 8 lshift rot 0 lshift or or
	1 6 lshift OUT0_CLR_REG io! \ CS low
    24 0 do
        dup
        1 23 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@ 2 rshift 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	 1 6 lshift OUT0_SET_REG io! \ CS high
	 $ff and  \ return only valid bits
;


 \ not tested!!!
: spi_adc9656_wr2 ( a b c )
	rot 23 lshift rot 8 lshift rot 0 lshift or or
	1 17 lshift OUT0_SET_REG io! \ OE buff active
	1 3 lshift OUT0_CLR_REG io! \ CS low
    24 0 do
        dup
        1 23 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@ 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	 1 3 lshift OUT0_SET_REG io! \ CS high
;


: spi_adc9656_rd1 ( a b c )
	rot 23 lshift rot 8 lshift rot 0 lshift or or
	1 17 lshift OUT0_SET_REG io! \ OE buff active
	1 3 lshift OUT0_CLR_REG io! \ CS high
    16 0 do
        dup
        1 23 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@  8 rshift 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	1 17 lshift OUT0_CLR_REG io! \ OE buff inactive
	8 0 do
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@  7 rshift 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	 1 3 lshift OUT0_SET_REG io! \ CS low
	 $ff and  \ return only valid bits
;


: spi_adc9656_rd2 ( a b c )
	rot 23 lshift rot 8 lshift rot 0 lshift or or
	1 18 lshift OUT0_SET_REG io! \ OE buff active
	1 4 lshift OUT0_CLR_REG io! \ CS high
    16 0 do
        dup
        1 23 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@  8 rshift 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	1 18 lshift OUT0_CLR_REG io! \ OE buff inactive
	8 0 do
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@  8 rshift 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	 1 4 lshift OUT0_SET_REG io! \ CS low
	 $ff and  \ return only valid bits
;



: spi_hmc7043_wr ( a b c )
	rot 23 lshift rot 8 lshift rot 0 lshift or or
	1 8 lshift OUT0_CLR_REG io! \ deactivate hmc830
	1 16 lshift OUT0_SET_REG io! \ OE buff active
	1 2 lshift OUT0_CLR_REG io! \ CS low
    24 0 do
        dup
        1 23 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@ 2 lshift 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	 1 2 lshift OUT0_SET_REG io! \ CS high
;

: spi_hmc7043_rd ( a b c )
	rot 23 lshift rot 8 lshift rot 0 lshift or or
	1 3 lshift OUT0_SET_REG io! \ deactivate hmc830
	1 16 lshift OUT0_SET_REG io! \ OE buff active
	1 2 lshift OUT0_CLR_REG io! \ CS high
    16 0 do
        dup
        1 23 lshift and
        if
           1 1 lshift OUT0_SET_REG io!
        else
           1 1 lshift OUT0_CLR_REG io!
        then
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@ 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	1 16 lshift OUT0_CLR_REG io! \ OE buff inactive
	8 0 do
        1 OUT0_SET_REG io! \ SCK hi
        2*
        INP0_REG io@ 1 and +
        1 OUT0_CLR_REG io! \ SCK low
    loop
	 1 2 lshift OUT0_SET_REG io! \ CS low
	 $ff and  \ return only valid bits
;





: hmc7043_cfg

	1 8 lshift OUT0_SET_REG io! \ hmc830 SEN high - protocol choice
	1 8 lshift OUT0_CLR_REG io! \ deactivate hmc830

 \  glbl_cfg1_swrst[0:0] = $0
 0 $0 $0 spi_hmc7043_wr .

 \  glbl_cfg1_sleep[0:0] = $1
 \  glbl_cfg1_restart[1:1] = $0
 \  sysr_cfg1_pulsor_req[2:2] = $0
 \  grpx_cfg1_mute[3:3] = $0
 \  dist_cfg1_perf_floor[6:6] = $1
 \  sysr_cfg1_reseed_req[7:7] = $0
 0 $1 $40 spi_hmc7043_wr .

 \  sysr_cfg1_rev[0:0] = $0
 \  sysr_cfg1_slipN_req[1:1] = $0
 0 $2 $0 spi_hmc7043_wr .

 \  glbl_cfg1_ena_sysr[2:2] = $1
 \  glbl_cfg2_ena_vcos[4:3] = $2
 \  glbl_cfg1_ena_sysri[5:5] = $1
 0 $3 $34 spi_hmc7043_wr .

 \  glbl_cfg7_ena_clkgr[6:0] = $7F
 0 $4 $7F spi_hmc7043_wr .

 \  glbl_cfg1_clear_alarms[0:0] = $0
 0 $6 $0 spi_hmc7043_wr .

 \  glbl_reserved[0:0] = $0
 0 $7 $0 spi_hmc7043_wr .

 \  glbl_cfg5_ibuf0_en[0:0] = $1
 \  glbl_cfg5_ibuf0_mode[4:1] = $3
 0 $A $7 spi_hmc7043_wr .

 \  glbl_cfg5_ibuf1_en[0:0] = $1
 \  glbl_cfg5_ibuf1_mode[4:1] = $3
 0 $B $7 spi_hmc7043_wr .

 \  glbl_cfg5_gpi1_en[0:0] = $0
 \  glbl_cfg5_gpi1_sel[4:1] = $0
 0 $46 $0 spi_hmc7043_wr .

 \  glbl_cfg8_gpo1_en[0:0] = $1
 \  glbl_cfg8_gpo1_mode[1:1] = $1
 \  glbl_cfg8_gpo1_sel[7:2] = $1
 0 $50 $7 spi_hmc7043_wr .

 \  glbl_cfg2_sdio_en[0:0] = $1
 \  glbl_cfg2_sdio_mode[1:1] = $1
 0 $54 $3 spi_hmc7043_wr .

 \  sysr_cfg3_pulsor_mode[2:0] = $0
 0 $5A $0 spi_hmc7043_wr .

 \  sysr_cfg1_synci_invpol[0:0] = $0
 \  sysr_cfg1_ext_sync_retimemode[2:2] = $1
 0 $5B $4 spi_hmc7043_wr .

 \  sysr_cfg16_divrat_lsb[7:0] = $0
 0 $5C $0 spi_hmc7043_wr .

 \  sysr_cfg16_divrat_msb[3:0] = $1
 0 $5D $1 spi_hmc7043_wr .

 \  dist_cfg1_extvco_islowfreq_sel[0:0] = $0
 \  dist_cfg1_extvco_div2_sel[1:1] = $0
 0 $64 $0 spi_hmc7043_wr .

 \  clkgrpx_cfg1_alg_dly_lowpwr_sel[0:0] = $0
 0 $65 $0 spi_hmc7043_wr .

 \  alrm_cfg1_sysr_unsyncd_allow[1:1] = $0
 \  alrm_cfg1_clkgrpx_validph_allow[2:2] = $0
 \  alrm_cfg1_sync_req_allow[4:4] = $1
 0 $71 $10 spi_hmc7043_wr .

 \  glbl_ro8_chipid_lob[7:0] = $1
 0 $78 $1 spi_hmc7043_wr .

 \  glbl_ro8_chipid_mid[7:0] = $2
 0 $79 $2 spi_hmc7043_wr .

 \  glbl_ro8_chipid_hib[7:0] = $3
 0 $7A $3 spi_hmc7043_wr .

 \  alrm_ro1_sysr_unsyncd_now[1:1] = $0
 \  alrm_ro1_clkgrpx_validph_now[2:2] = $1
 \  alrm_ro1_sync_req_now[4:4] = $0
 0 $7D $4 spi_hmc7043_wr .

 \  sysr_ro4_fsmstate[3:0] = $5
 \  grpx_ro1_outdivfsm_busy[4:4] = $0
 0 $91 $5 spi_hmc7043_wr .

 \  reg_98[7:0] = $0
 0 $98 $0 spi_hmc7043_wr .

 \  reg_99[7:0] = $0
 0 $99 $0 spi_hmc7043_wr .

 \  reg_9A[7:0] = $0
 0 $9A $0 spi_hmc7043_wr .

 \  reg_9B[7:0] = $AA
 0 $9B $AA spi_hmc7043_wr .

 \  reg_9C[7:0] = $AA
 0 $9C $AA spi_hmc7043_wr .

 \  reg_9D[7:0] = $AA
 0 $9D $AA spi_hmc7043_wr .

 \  reg_9E[7:0] = $AA
 0 $9E $AA spi_hmc7043_wr .

 \  reg_9F[7:0] = $55
 0 $9F $55 spi_hmc7043_wr .

 \  reg_A0[7:0] = $56
 0 $A0 $56 spi_hmc7043_wr .

 \  reg_A1[7:0] = $97
 0 $A1 $97 spi_hmc7043_wr .

 \  reg_A2[7:0] = $3
 0 $A2 $3 spi_hmc7043_wr .

 \  reg_A3[7:0] = $0
 0 $A3 $0 spi_hmc7043_wr .

 \  reg_A4[7:0] = $0
 0 $A4 $0 spi_hmc7043_wr .

 \  reg_AD[7:0] = $0
 0 $AD $0 spi_hmc7043_wr .

 \  reg_AE[7:0] = $8
 0 $AE $8 spi_hmc7043_wr .

 \  reg_AF[7:0] = $50
 0 $AF $50 spi_hmc7043_wr .

 \  reg_B0[7:0] = $9
 0 $B0 $9 spi_hmc7043_wr .

 \  reg_B1[7:0] = $D
 0 $B1 $D spi_hmc7043_wr .

 \  reg_B2[7:0] = $0
 0 $B2 $0 spi_hmc7043_wr .

 \  reg_B3[7:0] = $0
 0 $B3 $0 spi_hmc7043_wr .

 \  reg_B5[7:0] = $0
 0 $B5 $0 spi_hmc7043_wr .

 \  reg_B6[7:0] = $0
 0 $B6 $0 spi_hmc7043_wr .

 \  reg_B7[7:0] = $0
 0 $B7 $0 spi_hmc7043_wr .

 \  reg_B8[7:0] = $0
 0 $B8 $0 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg1_en[0:0] = $1
 \  clkgrp1_div1_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp1_div1_cfg2_startmode[3:2] = $0
 \  clkgrp1_div1_cfg1_rev[4:4] = $1
 \  clkgrp1_div1_cfg1_slipmask[5:5] = $1
 \  clkgrp1_div1_cfg1_reseedmask[6:6] = $0
 \  clkgrp1_div1_cfg1_hi_perf[7:7] = $1
 0 $C8 $B1 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg12_divrat_lsb[7:0] = $1
 0 $C9 $1 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg12_divrat_msb[3:0] = $0
 0 $CA $0 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg5_fine_delay[4:0] = $0
 0 $CB $0 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg5_sel_coarse_delay[4:0] = $0
 0 $CC $0 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg12_mslip_lsb[7:0] = $0
 0 $CD $0 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg12_mslip_msb[3:0] = $0
 0 $CE $0 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg2_sel_outmux[1:0] = $3
 \  clkgrp1_div1_cfg1_drvr_sel_testclk[2:2] = $0
 0 $CF $3 spi_hmc7043_wr .

 \  clkgrp1_div1_cfg5_drvr_res[1:0] = $1
 \  clkgrp1_div1_cfg5_drvr_spare[2:2] = $0
 \  clkgrp1_div1_cfg5_drvr_mode[4:3] = $1
 \  clkgrp1_div1_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp1_div1_cfg2_mutesel[7:6] = $0
 0 $D0 $9 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg1_en[0:0] = $1
 \  clkgrp1_div2_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp1_div2_cfg2_startmode[3:2] = $3
 \  clkgrp1_div2_cfg1_rev[4:4] = $1
 \  clkgrp1_div2_cfg1_slipmask[5:5] = $1
 \  clkgrp1_div2_cfg1_reseedmask[6:6] = $0
 \  clkgrp1_div2_cfg1_hi_perf[7:7] = $1
 0 $D2 $BD spi_hmc7043_wr .

 \  clkgrp1_div2_cfg12_divrat_lsb[7:0] = $E8
 0 $D3 $E8 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg12_divrat_msb[3:0] = $3
 0 $D4 $3 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg5_fine_delay[4:0] = $0
 0 $D5 $0 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg5_sel_coarse_delay[4:0] = $0
 0 $D6 $0 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg12_mslip_lsb[7:0] = $0
 0 $D7 $0 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg12_mslip_msb[3:0] = $0
 0 $D8 $0 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg2_sel_outmux[1:0] = $0
 \  clkgrp1_div2_cfg1_drvr_sel_testclk[2:2] = $0
 0 $D9 $0 spi_hmc7043_wr .

 \  clkgrp1_div2_cfg5_drvr_res[1:0] = $0
 \  clkgrp1_div2_cfg5_drvr_spare[2:2] = $0
 \  clkgrp1_div2_cfg5_drvr_mode[4:3] = $1
 \  clkgrp1_div2_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp1_div2_cfg2_mutesel[7:6] = $0
 0 $DA $8 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg1_en[0:0] = $1
 \  clkgrp2_div1_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp2_div1_cfg2_startmode[3:2] = $0
 \  clkgrp2_div1_cfg1_rev[4:4] = $1
 \  clkgrp2_div1_cfg1_slipmask[5:5] = $1
 \  clkgrp2_div1_cfg1_reseedmask[6:6] = $0
 \  clkgrp2_div1_cfg1_hi_perf[7:7] = $1
 0 $DC $B1 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg12_divrat_lsb[7:0] = $1
 0 $DD $1 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg12_divrat_msb[3:0] = $0
 0 $DE $0 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg5_fine_delay[4:0] = $0
 0 $DF $0 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg5_sel_coarse_delay[4:0] = $0
 0 $E0 $0 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg12_mslip_lsb[7:0] = $0
 0 $E1 $0 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg12_mslip_msb[3:0] = $0
 0 $E2 $0 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg2_sel_outmux[1:0] = $3
 \  clkgrp2_div1_cfg1_drvr_sel_testclk[2:2] = $0
 0 $E3 $3 spi_hmc7043_wr .

 \  clkgrp2_div1_cfg5_drvr_res[1:0] = $0
 \  clkgrp2_div1_cfg5_drvr_spare[2:2] = $0
 \  clkgrp2_div1_cfg5_drvr_mode[4:3] = $1
 \  clkgrp2_div1_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp2_div1_cfg2_mutesel[7:6] = $0
 0 $E4 $8 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg1_en[0:0] = $1
 \  clkgrp2_div2_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp2_div2_cfg2_startmode[3:2] = $0
 \  clkgrp2_div2_cfg1_rev[4:4] = $1
 \  clkgrp2_div2_cfg1_slipmask[5:5] = $1
 \  clkgrp2_div2_cfg1_reseedmask[6:6] = $0
 \  clkgrp2_div2_cfg1_hi_perf[7:7] = $1
 0 $E6 $B1 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg12_divrat_lsb[7:0] = $E8
 0 $E7 $E8 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg12_divrat_msb[3:0] = $3
 0 $E8 $3 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg5_fine_delay[4:0] = $0
 0 $E9 $0 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg5_sel_coarse_delay[4:0] = $0
 0 $EA $0 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg12_mslip_lsb[7:0] = $0
 0 $EB $0 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg12_mslip_msb[3:0] = $0
 0 $EC $0 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg2_sel_outmux[1:0] = $0
 \  clkgrp2_div2_cfg1_drvr_sel_testclk[2:2] = $0
 0 $ED $0 spi_hmc7043_wr .

 \  clkgrp2_div2_cfg5_drvr_res[1:0] = $0
 \  clkgrp2_div2_cfg5_drvr_spare[2:2] = $0
 \  clkgrp2_div2_cfg5_drvr_mode[4:3] = $1
 \  clkgrp2_div2_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp2_div2_cfg2_mutesel[7:6] = $0
 0 $EE $8 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg1_en[0:0] = $1
 \  clkgrp3_div1_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp3_div1_cfg2_startmode[3:2] = $0
 \  clkgrp3_div1_cfg1_rev[4:4] = $1
 \  clkgrp3_div1_cfg1_slipmask[5:5] = $1
 \  clkgrp3_div1_cfg1_reseedmask[6:6] = $0
 \  clkgrp3_div1_cfg1_hi_perf[7:7] = $1
 0 $F0 $B1 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg12_divrat_lsb[7:0] = $14
 0 $F1 $14 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg12_divrat_msb[3:0] = $0
 0 $F2 $0 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg5_fine_delay[4:0] = $0
 0 $F3 $0 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg5_sel_coarse_delay[4:0] = $0
 0 $F4 $0 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg12_mslip_lsb[7:0] = $0
 0 $F5 $0 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg12_mslip_msb[3:0] = $0
 0 $F6 $0 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg2_sel_outmux[1:0] = $0
 \  clkgrp3_div1_cfg1_drvr_sel_testclk[2:2] = $0
 0 $F7 $0 spi_hmc7043_wr .

 \  clkgrp3_div1_cfg5_drvr_res[1:0] = $1
 \  clkgrp3_div1_cfg5_drvr_spare[2:2] = $0
 \  clkgrp3_div1_cfg5_drvr_mode[4:3] = $1
 \  clkgrp3_div1_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp3_div1_cfg2_mutesel[7:6] = $0
 0 $F8 $9 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg1_en[0:0] = $1
 \  clkgrp3_div2_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp3_div2_cfg2_startmode[3:2] = $3
 \  clkgrp3_div2_cfg1_rev[4:4] = $1
 \  clkgrp3_div2_cfg1_slipmask[5:5] = $1
 \  clkgrp3_div2_cfg1_reseedmask[6:6] = $0
 \  clkgrp3_div2_cfg1_hi_perf[7:7] = $1
 0 $FA $BD spi_hmc7043_wr .

 \  clkgrp3_div2_cfg12_divrat_lsb[7:0] = $E8
 0 $FB $E8 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg12_divrat_msb[3:0] = $3
 0 $FC $3 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg5_fine_delay[4:0] = $0
 0 $FD $0 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg5_sel_coarse_delay[4:0] = $0
 0 $FE $0 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg12_mslip_lsb[7:0] = $0
 0 $FF $0 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg12_mslip_msb[3:0] = $0
 0 $100 $0 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg2_sel_outmux[1:0] = $2
 \  clkgrp3_div2_cfg1_drvr_sel_testclk[2:2] = $0
 0 $101 $2 spi_hmc7043_wr .

 \  clkgrp3_div2_cfg5_drvr_res[1:0] = $0
 \  clkgrp3_div2_cfg5_drvr_spare[2:2] = $0
 \  clkgrp3_div2_cfg5_drvr_mode[4:3] = $1
 \  clkgrp3_div2_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp3_div2_cfg2_mutesel[7:6] = $0
 0 $102 $8 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg1_en[0:0] = $1
 \  clkgrp4_div1_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp4_div1_cfg2_startmode[3:2] = $0
 \  clkgrp4_div1_cfg1_rev[4:4] = $1
 \  clkgrp4_div1_cfg1_slipmask[5:5] = $0
 \  clkgrp4_div1_cfg1_reseedmask[6:6] = $0
 \  clkgrp4_div1_cfg1_hi_perf[7:7] = $1
 0 $104 $91 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg12_divrat_lsb[7:0] = $14
 0 $105 $14 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg12_divrat_msb[3:0] = $0
 0 $106 $0 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg5_fine_delay[4:0] = $0
 0 $107 $0 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg5_sel_coarse_delay[4:0] = $0
 0 $108 $0 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg12_mslip_lsb[7:0] = $1
 0 $109 $1 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg12_mslip_msb[3:0] = $0
 0 $10A $0 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg2_sel_outmux[1:0] = $0
 \  clkgrp4_div1_cfg1_drvr_sel_testclk[2:2] = $0
 0 $10B $0 spi_hmc7043_wr .

 \  clkgrp4_div1_cfg5_drvr_res[1:0] = $1
 \  clkgrp4_div1_cfg5_drvr_spare[2:2] = $0
 \  clkgrp4_div1_cfg5_drvr_mode[4:3] = $1
 \  clkgrp4_div1_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp4_div1_cfg2_mutesel[7:6] = $0
 0 $10C $9 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg1_en[0:0] = $1
 \  clkgrp4_div2_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp4_div2_cfg2_startmode[3:2] = $3
 \  clkgrp4_div2_cfg1_rev[4:4] = $1
 \  clkgrp4_div2_cfg1_slipmask[5:5] = $1
 \  clkgrp4_div2_cfg1_reseedmask[6:6] = $0
 \  clkgrp4_div2_cfg1_hi_perf[7:7] = $1
 0 $10E $BD spi_hmc7043_wr .

 \  clkgrp4_div2_cfg12_divrat_lsb[7:0] = $E8
 0 $10F $E8 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg12_divrat_msb[3:0] = $3
 0 $110 $3 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg5_fine_delay[4:0] = $0
 0 $111 $0 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg5_sel_coarse_delay[4:0] = $0
 0 $112 $0 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg12_mslip_lsb[7:0] = $0
 0 $113 $0 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg12_mslip_msb[3:0] = $0
 0 $114 $0 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg2_sel_outmux[1:0] = $2
 \  clkgrp4_div2_cfg1_drvr_sel_testclk[2:2] = $0
 0 $115 $2 spi_hmc7043_wr .

 \  clkgrp4_div2_cfg5_drvr_res[1:0] = $0
 \  clkgrp4_div2_cfg5_drvr_spare[2:2] = $0
 \  clkgrp4_div2_cfg5_drvr_mode[4:3] = $1
 \  clkgrp4_div2_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp4_div2_cfg2_mutesel[7:6] = $0
 0 $116 $8 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg1_en[0:0] = $1
 \  clkgrp5_div1_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp5_div1_cfg2_startmode[3:2] = $0
 \  clkgrp5_div1_cfg1_rev[4:4] = $1
 \  clkgrp5_div1_cfg1_slipmask[5:5] = $1
 \  clkgrp5_div1_cfg1_reseedmask[6:6] = $0
 \  clkgrp5_div1_cfg1_hi_perf[7:7] = $1
 0 $118 $B1 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg12_divrat_lsb[7:0] = $14
 0 $119 $14 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg12_divrat_msb[3:0] = $0
 0 $11A $0 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg5_fine_delay[4:0] = $0
 0 $11B $0 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg5_sel_coarse_delay[4:0] = $0
 0 $11C $0 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg12_mslip_lsb[7:0] = $0
 0 $11D $0 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg12_mslip_msb[3:0] = $0
 0 $11E $0 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg2_sel_outmux[1:0] = $0
 \  clkgrp5_div1_cfg1_drvr_sel_testclk[2:2] = $0
 0 $11F $0 spi_hmc7043_wr .

 \  clkgrp5_div1_cfg5_drvr_res[1:0] = $1
 \  clkgrp5_div1_cfg5_drvr_spare[2:2] = $0
 \  clkgrp5_div1_cfg5_drvr_mode[4:3] = $1
 \  clkgrp5_div1_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp5_div1_cfg2_mutesel[7:6] = $0
 0 $120 $9 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg1_en[0:0] = $1
 \  clkgrp5_div2_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp5_div2_cfg2_startmode[3:2] = $2
 \  clkgrp5_div2_cfg1_rev[4:4] = $1
 \  clkgrp5_div2_cfg1_slipmask[5:5] = $1
 \  clkgrp5_div2_cfg1_reseedmask[6:6] = $0
 \  clkgrp5_div2_cfg1_hi_perf[7:7] = $1
 0 $122 $B9 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg12_divrat_lsb[7:0] = $14
 0 $123 $14 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg12_divrat_msb[3:0] = $0
 0 $124 $0 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg5_fine_delay[4:0] = $0
 0 $125 $0 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg5_sel_coarse_delay[4:0] = $0
 0 $126 $0 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg12_mslip_lsb[7:0] = $0
 0 $127 $0 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg12_mslip_msb[3:0] = $0
 0 $128 $0 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg2_sel_outmux[1:0] = $2
 \  clkgrp5_div2_cfg1_drvr_sel_testclk[2:2] = $0
 0 $129 $2 spi_hmc7043_wr .

 \  clkgrp5_div2_cfg5_drvr_res[1:0] = $0
 \  clkgrp5_div2_cfg5_drvr_spare[2:2] = $0
 \  clkgrp5_div2_cfg5_drvr_mode[4:3] = $1
 \  clkgrp5_div2_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp5_div2_cfg2_mutesel[7:6] = $0
 0 $12A $8 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg1_en[0:0] = $1
 \  clkgrp6_div1_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp6_div1_cfg2_startmode[3:2] = $3
 \  clkgrp6_div1_cfg1_rev[4:4] = $1
 \  clkgrp6_div1_cfg1_slipmask[5:5] = $1
 \  clkgrp6_div1_cfg1_reseedmask[6:6] = $0
 \  clkgrp6_div1_cfg1_hi_perf[7:7] = $1
 0 $12C $BD spi_hmc7043_wr .

 \  clkgrp6_div1_cfg12_divrat_lsb[7:0] = $14
 0 $12D $14 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg12_divrat_msb[3:0] = $0
 0 $12E $0 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg5_fine_delay[4:0] = $0
 0 $12F $0 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg5_sel_coarse_delay[4:0] = $0
 0 $130 $0 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg12_mslip_lsb[7:0] = $0
 0 $131 $0 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg12_mslip_msb[3:0] = $0
 0 $132 $0 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg2_sel_outmux[1:0] = $0
 \  clkgrp6_div1_cfg1_drvr_sel_testclk[2:2] = $0
 0 $133 $0 spi_hmc7043_wr .

 \  clkgrp6_div1_cfg5_drvr_res[1:0] = $1
 \  clkgrp6_div1_cfg5_drvr_spare[2:2] = $0
 \  clkgrp6_div1_cfg5_drvr_mode[4:3] = $1
 \  clkgrp6_div1_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp6_div1_cfg2_mutesel[7:6] = $0
 0 $134 $9 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg1_en[0:0] = $1
 \  clkgrp6_div2_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp6_div2_cfg2_startmode[3:2] = $3
 \  clkgrp6_div2_cfg1_rev[4:4] = $1
 \  clkgrp6_div2_cfg1_slipmask[5:5] = $1
 \  clkgrp6_div2_cfg1_reseedmask[6:6] = $1
 \  clkgrp6_div2_cfg1_hi_perf[7:7] = $1
 0 $136 $FD spi_hmc7043_wr .

 \  clkgrp6_div2_cfg12_divrat_lsb[7:0] = $14
 0 $137 $14 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg12_divrat_msb[3:0] = $0
 0 $138 $0 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg5_fine_delay[4:0] = $0
 0 $139 $0 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg5_sel_coarse_delay[4:0] = $0
 0 $13A $0 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg12_mslip_lsb[7:0] = $0
 0 $13B $0 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg12_mslip_msb[3:0] = $0
 0 $13C $0 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg2_sel_outmux[1:0] = $2
 \  clkgrp6_div2_cfg1_drvr_sel_testclk[2:2] = $0
 0 $13D $2 spi_hmc7043_wr .

 \  clkgrp6_div2_cfg5_drvr_res[1:0] = $0
 \  clkgrp6_div2_cfg5_drvr_spare[2:2] = $0
 \  clkgrp6_div2_cfg5_drvr_mode[4:3] = $1
 \  clkgrp6_div2_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp6_div2_cfg2_mutesel[7:6] = $0
 0 $13E $8 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg1_en[0:0] = $1
 \  clkgrp7_div1_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp7_div1_cfg2_startmode[3:2] = $3
 \  clkgrp7_div1_cfg1_rev[4:4] = $1
 \  clkgrp7_div1_cfg1_slipmask[5:5] = $1
 \  clkgrp7_div1_cfg1_reseedmask[6:6] = $1
 \  clkgrp7_div1_cfg1_hi_perf[7:7] = $1
 0 $140 $FD spi_hmc7043_wr .

 \  clkgrp7_div1_cfg12_divrat_lsb[7:0] = $14
 0 $141 $14 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg12_divrat_msb[3:0] = $0
 0 $142 $0 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg5_fine_delay[4:0] = $0
 0 $143 $0 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg5_sel_coarse_delay[4:0] = $0
 0 $144 $0 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg12_mslip_lsb[7:0] = $0
 0 $145 $0 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg12_mslip_msb[3:0] = $0
 0 $146 $0 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg2_sel_outmux[1:0] = $0
 \  clkgrp7_div1_cfg1_drvr_sel_testclk[2:2] = $0
 0 $147 $0 spi_hmc7043_wr .

 \  clkgrp7_div1_cfg5_drvr_res[1:0] = $1
 \  clkgrp7_div1_cfg5_drvr_spare[2:2] = $0
 \  clkgrp7_div1_cfg5_drvr_mode[4:3] = $1
 \  clkgrp7_div1_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp7_div1_cfg2_mutesel[7:6] = $0
 0 $148 $9 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg1_en[0:0] = $1
 \  clkgrp7_div2_cfg1_phdelta_mslip[1:1] = $0
 \  clkgrp7_div2_cfg2_startmode[3:2] = $3
 \  clkgrp7_div2_cfg1_rev[4:4] = $1
 \  clkgrp7_div2_cfg1_slipmask[5:5] = $1
 \  clkgrp7_div2_cfg1_reseedmask[6:6] = $1
 \  clkgrp7_div2_cfg1_hi_perf[7:7] = $1
 0 $14A $FD spi_hmc7043_wr .

 \  clkgrp7_div2_cfg12_divrat_lsb[7:0] = $0
 0 $14B $0 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg12_divrat_msb[3:0] = $1
 0 $14C $1 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg5_fine_delay[4:0] = $0
 0 $14D $0 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg5_sel_coarse_delay[4:0] = $0
 0 $14E $0 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg12_mslip_lsb[7:0] = $0
 0 $14F $0 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg12_mslip_msb[3:0] = $0
 0 $150 $0 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg2_sel_outmux[1:0] = $2
 \  clkgrp7_div2_cfg1_drvr_sel_testclk[2:2] = $0
 0 $151 $2 spi_hmc7043_wr .

 \  clkgrp7_div2_cfg5_drvr_res[1:0] = $0
 \  clkgrp7_div2_cfg5_drvr_spare[2:2] = $0
 \  clkgrp7_div2_cfg5_drvr_mode[4:3] = $1
 \  clkgrp7_div2_cfg_outbuf_dyn[5:5] = $0
 \  clkgrp7_div2_cfg2_mutesel[7:6] = $0
 0 $152 $8 spi_hmc7043_wr .



;


16 base !

: config_hmc_830
1 8 lshift OUT0_SET_REG io! \ SEN high - protocol selection
1 8 lshift OUT0_SET_REG io! \ SEN low


0 $0 $20 spi_hmc830 .
 \ 0 $0 $00 spi_hmc830 .
0 $1 $2 spi_hmc830 .  \ chip enable by SPI 
0 $2 $2 spi_hmc830 .  \ divider = 2 
0 $6 $303CA spi_hmc830 . 
 \ 0 $9 $153FFF spi_hmc830 . 
0 $5 $1628 spi_hmc830 . 
0 $5 $60A0 spi_hmc830 . 
0 $5 $E090 spi_hmc830 . 
0 $5 $2818 spi_hmc830 . 
0 $5 $F88  spi_hmc830 . \ magic word enables PLL out
0 $5 $0 spi_hmc830 . 
0 $7 $14d spi_hmc830 . 
0 $8 $c1beff spi_hmc830 . 
0 $a $2046 spi_hmc830 . 
0 $b $7c061 spi_hmc830 . 
0 $f $81 spi_hmc830 . 
\ 0 $3 38 spi_hmc830 . 
\ 0 $4 3851ec spi_hmc830 . 
;


\ Procedures for selecting the bus handled by the J1B I2C controller
: bus_sel ( n_bus )
    I2C_BUS_SEL io!
;

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
100000000  $10000000 decimal um* 2constant 100E6*1<<28
4850 1000000 um* 2constant S5_FDCOL
5670 1000000 um* 2constant S5_FDCOH

create s5_hsdvs  11  , 9 ,  7 , 6 , 5 , 4 ,
\ The implementation is split between a few words for easier testing

: Si57x_read_setgs
    \ Reset Si57x to initial settings
    $87 $01 Si57x_wr
    \ read RFREQ and HSDIV
    7 Si57x_rd ( frq r7 )
    dup 5 rshift 4 + S5_HSDIV !
    $1f and 2 lshift ( frq N1[6:2] )
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
    $3f and S5_RFREQ c! \ Store bits 39(37):32
    9 Si57x_rd ( frq r9 )
    S5_RFREQ 7 + c! \ Bits 31:24
    $a Si57x_rd ( frq r10 )
    S5_RFREQ 6 + c! \ Bits 23:16
    $b Si57x_rd ( frq r11 )
    S5_RFREQ 5 + c! \ Bits 15:8
    $c Si57x_rd ( frq r12 )
    S5_RFREQ 4 + c! \ Bits 7:0
;

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
    S5_FDCO 2@ 1 28 lshift S5_FXTAL @ M*/ S5_RFREQ 2!    
;    

: Si57x_write_setgs
    $89 $10 Si57x_wr
    $87 $30 Si57x_wr
    S5_N1 @ 1-  2 rshift
    S5_HSDIV @ 4 - 5 lshift or
    7 swap Si57x_wr    ( n1-1 )
    S5_RFREQ 4 + c@ \ Bits 7:0
    12 swap Si57x_wr ( frq r12 )
    S5_RFREQ 5 + c@ \ Bits 15:8
    11 swap Si57x_wr ( frq r11 )
    S5_RFREQ 6 + c@ \ Bits 23:16
    10 swap Si57x_wr ( frq r10 )
    S5_RFREQ 7 + c@ \ Bits 31:24
    9 swap Si57x_wr ( frq r9 )
    S5_N1 @ 1- 6 lshift
    S5_RFREQ c@ $3f and or 8 swap Si57x_wr
    $89 0 Si57x_wr
    $87 $40 Si57x_wr
;    

: Si57x_SetFrq ( frq -- )
    4 bus_sel \ We must select the 4th bus
    \ Save old mux and set mux to access Si57x
    I2C_MUX i2c_rd1 Si57x_old_mux !
    I2C_MUX_Si57x I2C_MUX i2c_wr1
    Si57x_read_setgs
    Si57x_calc_setgs
    Si57x_write_setgs
    Si57x_old_mux @ I2C_MUX i2c_wr1
;    

\ Procedures to control the clock generator in the FM-S14 FMC board
110 constant FMS14Q_ADR
: FMS14Q_wr ( addr val -- )
    FMS14Q_ADR I2C_ind_wr
;

: FMS14Q_rd ( addr -- val )
    FMS14Q_ADR I2C_ind_rd
;

\ Variables
variable S14_CP0
variable S14_N0
variable S14_M0 \ It is stored multiplied by 1 << 18
create S14_PVs 1 , 2 , 4 , 5 ,
variable S14_P0
variable S14_P0V
2variable S14_FVCO
2variable S14_FREF

212500000 constant S14_FOUT0
1950 1000000 um* 2constant S14_FVCOL
2600 1000000 um* 2constant S14_FVCOH

: FMS14Q_sim_read ( -- )
    \ Read settings for config 0
    4874232 S14_M0 ! 10 S14_N0 !
    S14_FOUT0 1 18 lshift UM* ( fout0*[1<<18] )
    S14_N0 @ S14_M0 @ m*/ ( fref0 . )
    S14_FREF 2! ( )
    \ Print results
    ." S14_M0*2^18=" S14_M0 @ .
    ." S14_N0=" S14_N0 @ .
    ." S14_FREF0=" S14_FREF 2@ d.
;

: FMS14Q_read_setgs ( -- )
    \ Read settings for config 0
    0 FMS14Q_rd ( r0 )
    dup 6 rshift S14_CP0 !
    $3f and 17 lshift S14_M0 !
    4 FMS14Q_rd ( r4 )
    9 lshift S14_M0 @ or S14_M0 !
    8 FMS14Q_rd ( r8 )
    1 lshift S14_M0 @ or S14_M0 !
    12 FMS14Q_rd ( r12 )
    dup 7 rshift S14_M0 @ or S14_M0 !
    $7f and S14_N0 !
    20 FMS14Q_rd ( r20 )
    dup 6 rshift ( r20 P0 )
    \ Translate P0 into P0V
    S14_PVs + c@ S14_P0V !
    32 and 23 5 - lshift S14_M0 @ or S14_M0 !
    \ Calculate FREF0 
    S14_FOUT0 1 18 lshift UM* ( fout0*[1<<18] )
    S14_N0 @ S14_M0 @ m*/ ( fref0 . )
    S14_FREF 2! ( )
    \ Print results
    ." S14_M0*2^18=" S14_M0 @ .
    ." S14_N0=" S14_N0 @ .
    ." S14_FREF0=" S14_FREF 2@ d.
;
: FMS14Q_calc_setgs ( frq )
\ Now we find the right divisor
    4 0 do ( frq )
	\ Get PV
	i S14_P0 !
	2 S14_N0 !
	1 0 do ( frq )
	    S14_PVs S14_P0 @ cells + @ dup S14_P0V ! ( frq pv )
	    S14_N0 @ ( frq pv N )
	    $7e over < if
		drop drop
		leave ( frq ) 
	    then ( frq pv N )
	    \ Calculate fvco
	    m* ( frq pv*N . )
	    2 pick 1 ( frq pv*N . frq 1 )
	    m*/ ( frq fvco . )
	    2dup S14_FVCO 2!
	    \ 2dup d. cr
	    2dup S14_FVCOL D< if
		2drop 
	    else
		S14_FVCOH  D< if
		    ." Found! "
		    leave
		then
	    then ( frq )
	    \ Update N
	    S14_N0 @
	    dup 6 < if
		1+
	    else
		2 +
	    then
	    S14_N0 !
	0 +loop ( frq )
	S14_N0 @ 127 < if
	    \ It means that the proper value was found!
	    leave
	then
	.s
    loop ( frq )
    \ Calculate M=FVCO/FREF to get the value properly scaled, multiply FVCO first by 1<<18)
    drop ( ) \ Dropped frq
    S14_FVCO 2@ 
    2dup ." fvco=" d.
    1 18 lshift 0 ud* ( )
    .UDres
    S14_FREF 2@ 
    2dup ." fref=" d.
    ud/ ( M . )
    $ffffff. 2over d< if
	." Lower  M is too big " d.
	$87 throw 
    then
    2dup ." M=" d.
    drop \ Drop higher word
    S14_M0 !
;

: FMS14Q_write_setgs ( -- )
    \ So now we are ready to write the results, copying other settings from channel 0
    S14_M0 @
    S14_CP0 @ 6 lshift
    over $7e0000 and 17 rshift or 
    3 swap FMS14Q_wr ( M )
    dup 9 rshift $ff and
    7 swap FMS14Q_wr ( M )
    dup 1 rshift $ff and
    11 swap FMS14Q_wr ( M )
    dup 1 and 7 lshift
    S14_N0 @ $7f and or
    15 swap FMS14Q_wr ( M )
    20 FMS14Q_rd $1f and
    S14_P0 @ 6 lshift or
    swap 800000 and 23 5 - rshift or    
    23 swap FMS14Q_wr ( )
    \ Now toggle the FSEL bits
    18 FMS14Q_rd
    dup $e7 and
    18 swap FMS14Q_wr
    $18 or
    18 swap FMS14Q_wr
;

: FMS14Q_SetFrq ( frq -- )
    FMS14Q_read_setgs
    FMS14Q_calc_setgs
    FMS14Q_write_setgs
;


\ Procedures to control the clock matrix

: ClkMtx_SetOut ( n_in n_out -- )
    over -1 < ( n_in n_out flag )
    2 pick 15 > or if
	." n_in must be between -1 and 15 "
	$83 throw
    then ( n_in n_out )
    dup 0 < over 15 > or if
	." n_out must be between 0 and 15 "
	$83 throw
    then ( n_in n_out )
    4 bus_sel
    I2C_MUX i2c_rd1 Si57x_old_mux !
    I2C_MUX_ADN4604 I2C_MUX i2c_wr1
    over -1 = if 
	\ Switch off the output
	$20 + 0 ADN4604_wr drop 
    else
	\ Select the input and switch the output
	\ Number of register as 0x90+n_out/2
	\ Number of nibble as n_out % 2
	\ Switch according to the nibble
	dup 1 and if
	    swap 4 lshift
	    $0f ( n_out n_in*16 mask )
	else
	    swap
	    $f0 ( n_out n_in mask )
	then ( n_out n_in_shifted mask )
	\ Calculate the number of register
	rot ( n_in_shifted mask n_out )
	dup >r  ( n_in_shifted mask n_out ) ( R: n_out)
	1 rshift $90 + >r ( n_in_shifted mask ) ( R: n_out reg_adr )
	r@ ADN4604_rd ( n_in_shifted mask old_val)
	and or ( new_val ) ( R: n_out reg_adr )
	r> swap ADN4604_wr
	\ Trigger matrix update
	$81 $00 ADN4604_wr
	$80 $01 ADN4604_wr
	\ Switch on the output
	r> $20 + $30 ADN4604_wr
    then
    Si57x_old_mux @ I2C_MUX i2c_wr1
;

\ Procedure for reading the EUI from AT24MAC602
9 buffer: EUI_buf

$58 constant AT24MAC
12 constant AT24MAC_i2c_sel

: EUI_read
    4 bus_sel
    AT24MAC_i2c_sel I2C_MUX i2c_wr1
    $98 AT24MAC i2c_wr1
    EUI_buf 8 AT24MAC i2c_rd
;

: .bytebuf ( addr count -- )
    0 do ( addr )
	dup i + c@ .
    loop
    drop ( )
;

\ Correct initialization of the I2C controller and I2C bus switch
: AFCK_i2c_init
4 bus_sel i2c_init I2C_MUX i2c_rd1 .
;

