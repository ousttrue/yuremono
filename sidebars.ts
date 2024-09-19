import type { SidebarsConfig } from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  sidebar: [
    'index',
    {
      type: 'category',
      label: '試作',
      link: { type: 'doc', id: 'proto/index' },
      items: [
      ],
    },
    {
      type: 'category',
      label: '実装案1(StrandCloth)',
      link: { type: 'doc', id: 'impl/index' },
      items: [
        'impl/head_tail',
        'impl/timedelta',
        'impl/stiffness',
        'impl/dragforce',
        'impl/externalforce',
        'impl/verlet',
        'impl/collision',
        'impl/constraint_update',
        'impl/cloth',
      ],
    },
    {
      type: 'category',
      label: '実装案2(RotateParticle)',
      link: { type: 'doc', id: 'impl2/index' },
      items: [
        'impl2/parameters',
        'impl2/sequence',
      ]
    },
    {
      type: 'category',
      label: '質点力学',
      link: { type: 'doc', id: 'particle/index' },
      items: [
        'particle/verlet',
      ]
    },
    // springbone
    {
      type: 'category',
      label: 'SpringBone',
      link: { type: 'doc', id: 'springbone/index' },
      items: [
        'springbone/rocketjump',
        'springbone/vrmspringbone',
      ],
    },
    // cloth
    {
      type: 'category',
      label: 'Cloth',
      items: [
        'cloth/index',
        {
          type: 'category',
          label: '実装例',
          link: { type: 'doc', id: 'cloth/impl/index' },
          items: [
            'cloth/impl/cloth_yr',
            'cloth/impl/cloth_pbd',
          ]
        },
      ],
    },
    {
      type: 'category',
      label: 'Collision',
      items: [
        'collision/index',
      ]
    },
    {
      type: 'category',
      label: '参考',
      items: [
        'refrence/pbd',
        'refrence/xpbd',
        'refrence/mass_spring_model',
      ]
    },
  ],
};

export default sidebars;
