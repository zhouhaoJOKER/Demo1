1.消息通知-单据创建
 第一步 ：首先中台这边会有表记录消息到数据库中【OrderCreate】这里面记录的是蜻蜓平台的订单
 第二步 ：通过 【OrderCreate】这个表的数据，轮询的查单据的明细，然后同步到线下来。
          记录到中台这边的表分别是：
          a. JTSND(商品退货通知单 审核2状态)
          b. SG_Gathering fQuantity>0 (小票表) 销货数据 -> 并更新isonline='1'
          b. SG_Gathering fQuantity<0 (小票表) 退货数据 -> 并更新isonline='1'
          此时单据生成的流程已经完结 并未更新实际库存
2.消息通知-库存变更 （备注：以上1和2流程是具有关联性）
  第一步：首先中台这边会记录消息到数据库中【ItemAmountChanged】这个记录的就是变更的记录 
          同时这个里面的单据号也是和上面的对应的
  第二步：通过【ItemAmountChanged】这个表的数据。直接生成了以下业务单据
          a.SHTHD BYZD3 会关联到上面生成通知单 ，然后这单据也是验收状态 产生库存变化
          b.SPJHD 这个是蜻蜓要求我们这边直接生成，并且验收状态 产生库存变化
          c.LSXHD/LSTHD YDJH记录到的就是此前SG_Gathering表中的小票编号，验收状态，然后明细数量和小票明细数量一致，
          那就更新小票的日结状态 

3.库存同步接口 慎用，接口是已经完成

4.中台创建单据 这类简单归纳 上传单据三类：
  a. SG_Gathering is_move='0' isonline='0' fQuantity>0 (小票表) 销货数据
  b. SG_Gathering is_move='0' isonline='0' fQuantity<0 (小票表) 退货数据
  c. SPTHD SH='1' is_move='0' isonline='0' 采购退货单 
  上传单据
5.中台库存变更 以上单据：
  a. SG_Gathering is_move='1' isonline='0' fQuantity>0 (小票表) 销货数据
  b. SG_Gathering is_move='1' isonline='0' fQuantity<0 (小票表) 退货数据
  c. SPTHD SH='1' is_move='1' isonline='0' 采购退货单 
  d. SPJHD SH='1' is_move='0' isonline='0' 采购进货单
           
6.其他档案同步接口
7.应用授权开发

